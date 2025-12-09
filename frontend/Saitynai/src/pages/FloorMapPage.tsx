import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { BackButton } from '../components/BackButton';
import { useAuth } from '../contexts/AuthContext';
import { getFloor } from '../services/floorService';
import { createPoint, deletePoint, getPoints } from '../services/pointService';
import type { Floor, Point } from '../types/api';
import './style/FloorMapPage.css';

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8090';

type MapMode = 'view' | 'add' | 'remove';

const modeLabels: Record<MapMode, string> = {
  view: 'View',
  add: 'Add',
  remove: 'Remove',
};

export default function FloorMapPage() {
  const { floorId } = useParams();
  const navigate = useNavigate();
  const parsedFloorId = Number(floorId);

  const { permissionScopes, hasPermission, hasPermissionForId } = useAuth();

  const [floor, setFloor] = useState<Floor | null>(null);
  const [points, setPoints] = useState<Point[]>([]);
  const [selectedPoint, setSelectedPoint] = useState<Point | null>(null);
  const [mode, setMode] = useState<MapMode>('view');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pointToDelete, setPointToDelete] = useState<Point | null>(null);

  const containerRef = useRef<HTMLDivElement | null>(null);
  const imageRef = useRef<HTMLImageElement | null>(null);
  const [naturalSize, setNaturalSize] = useState<{ width: number; height: number } | null>(null);
  const [scale, setScale] = useState<{ x: number; y: number }>({ x: 1, y: 1 });

  useEffect(() => {
    if (Number.isNaN(parsedFloorId)) {
      setError('Invalid floor id');
      setIsLoading(false);
      return;
    }

    const load = async () => {
      try {
        const [floorResp, pointsResp] = await Promise.all([
          getFloor(parsedFloorId),
          getPoints(),
        ]);

        setFloor(floorResp.data);
        setPoints(pointsResp.data.filter((p) => p.floorId === parsedFloorId));
      } catch (err: any) {
        const message = err?.response?.data?.message || err?.message || 'Failed to load floor map';
        setError(message);
      } finally {
        setIsLoading(false);
      }
    };

    load();
  }, [parsedFloorId]);

  const updateScale = useCallback(() => {
    if (!containerRef.current || !naturalSize) return;
    const rect = containerRef.current.getBoundingClientRect();
    if (rect.width === 0 || rect.height === 0) return;
    setScale({
      x: rect.width / naturalSize.width,
      y: rect.height / naturalSize.height,
    });
  }, [naturalSize]);

  useEffect(() => {
    const observer = new ResizeObserver(() => updateScale());
    if (containerRef.current) {
      observer.observe(containerRef.current);
    }
    return () => observer.disconnect();
  }, [updateScale]);

  const handleImageLoad = () => {
    if (!imageRef.current) return;
    const { naturalWidth, naturalHeight } = imageRef.current;
    setNaturalSize({ width: naturalWidth, height: naturalHeight });
    // Allow layout to settle before calculating scale
    requestAnimationFrame(updateScale);
  };

  const handleCanvasClick = async (event: React.MouseEvent<HTMLDivElement>) => {
    if (mode !== 'add' || !naturalSize) return;
    if (!containerRef.current) return;

    const rect = containerRef.current.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    if (scale.x === 0 || scale.y === 0) return;

    const originalX = Math.round(x / scale.x);
    const originalY = Math.round(y / scale.y);

    try {
      const resp = await createPoint({
        floorId: parsedFloorId,
        latitude: originalY,
        longitude: originalX,
        apCount: 0,
      });
      setPoints((prev) => [...prev, resp.data]);
      setSelectedPoint(resp.data);
      setMode('view');
    } catch (err: any) {
      const message = err?.response?.data?.message || err?.message || 'Failed to create point';
      setError(message);
    }
  };

  const handlePointClick = (point: Point, e: React.MouseEvent) => {
    e.stopPropagation();
    if (mode === 'remove') {
      setPointToDelete(point);
      return;
    }

    setSelectedPoint(point);
    setMode('view');
  };

  const confirmDeletePoint = async () => {
    if (!pointToDelete) return;
    try {
      await deletePoint(pointToDelete.id);
      setPoints((prev) => prev.filter((p) => p.id !== pointToDelete.id));
      setSelectedPoint(null);
      setPointToDelete(null);
      setMode('view');
    } catch (err: any) {
      const message = err?.response?.data?.message || err?.message || 'Failed to delete point';
      setError(message);
    }
  };

  const getPointColor = (apCount: number): string => {
    if (apCount === 0) return '#ef4444'; // Red: no scans
    if (apCount <= 10) return '#eab308'; // Yellow: 1-10 scans
    return '#22c55e'; // Green: 11+ scans
  };

  const floorImageUrl = floor?.floorPlanPath ? `${apiBaseUrl}${floor.floorPlanPath}` : null;

  const pointStyles = useMemo(() => {
    return points.map((p) => ({
      id: p.id,
      left: p.longitude * scale.x,
      top: p.latitude * scale.y,
      point: p,
    }));
  }, [points, scale]);

  // Permission gating mirrors PointsPage logic but scoped to the active floor
  const pointCreateScope = useMemo(() => (
    permissionScopes.find((s) => s.permissionName === 'create' && s.resourceType === 'Point')
  ), [permissionScopes]);
  const pointDeleteScope = useMemo(() => (
    permissionScopes.find((s) => s.permissionName === 'delete' && s.resourceType === 'Point')
  ), [permissionScopes]);

  const cascadeFloorsForCreate = pointCreateScope?.cascadeFrom?.['Floor'] || [];
  const cascadeFloorsForDelete = pointDeleteScope?.cascadeFrom?.['Floor'] || [];
  const cascadeBuildingsForCreate = pointCreateScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForDelete = pointDeleteScope?.cascadeFrom?.['Building'] || [];

  const canCreate = () => {
    if (hasPermission('create', 'Point')) return true;
    if (!Number.isNaN(parsedFloorId) && cascadeFloorsForCreate.includes(parsedFloorId)) return true;
    if (floor?.buildingId !== undefined && cascadeBuildingsForCreate.includes(floor.buildingId)) return true;
    return false;
  };

  const canDelete = (point?: Point) => {
    if (point && hasPermissionForId('delete', 'Point', point.id)) return true;
    if (!Number.isNaN(parsedFloorId) && cascadeFloorsForDelete.includes(parsedFloorId)) return true;
    if (floor?.buildingId !== undefined && cascadeBuildingsForDelete.includes(floor.buildingId)) return true;
    return false;
  };

  const disabledReason = () => {
    if (mode === 'add' && !canCreate()) return 'Add disabled: no permission';
    if (mode === 'remove' && !canDelete()) return 'Remove disabled: no permission';
    if (!floorImageUrl) return 'No floor plan uploaded';
    return null;
  };

  if (isLoading) {
    return <div className="map-page-wrapper">Loading map...</div>;
  }

  if (error) {
    return (
      <div className="map-page-wrapper">
        <BackButton label="Back to Floors" onClick={() => navigate(-1)} />
        <div className="map-error">{error}</div>
      </div>
    );
  }

  return (
    <div className="map-page-wrapper">
      <div className="map-page-header">
        <BackButton
          label="Back to Floors"
          onClick={() => {
            if (floor) {
              navigate(`/buildings/${floor.buildingId}/floors`);
            } else {
              navigate('/buildings');
            }
          }}
        />
        <div>
          <h2>Floor #{parsedFloorId} Map</h2>
          <p className="map-subtitle">Click the map to add, or select a point to view/remove.</p>
        </div>
      </div>

      <div className="map-controls">
        {(['view', 'add', 'remove'] as MapMode[]).map((m) => (
          <button
            key={m}
            className={`map-mode-button ${mode === m ? 'active' : ''}`}
            onClick={() => setMode(m)}
            disabled={(m === 'add' && !canCreate()) || (m === 'remove' && !canDelete())}
          >
            {modeLabels[m]}
          </button>
        ))}
        <div className="map-status">
          <span className="status-dot" data-mode={mode} />
          <span>{mode === 'view' ? 'View points' : mode === 'add' ? 'Add point by clicking' : 'Remove by clicking points'}</span>
          {disabledReason() && <span className="muted"> · {disabledReason()}</span>}
        </div>
      </div>

      {!floorImageUrl && (
        <div className="map-error">No floor plan uploaded for this floor.</div>
      )}

      {floorImageUrl && (
        <div
          className="map-canvas"
          ref={containerRef}
          onClick={handleCanvasClick}
        >
          <img
            ref={imageRef}
            src={floorImageUrl}
            alt={`Floor ${parsedFloorId} plan`}
            onLoad={handleImageLoad}
            onError={() => setError('Failed to load floor plan image')}
          />
          {pointStyles.map(({ id, left, top, point }) => (
            <button
              key={id}
              className={`map-point ${selectedPoint?.id === id ? 'selected' : ''}`}
              style={{ left, top, background: getPointColor(point.apCount) }}
              onClick={(e) => handlePointClick(point, e)}
              title={`Point #${id} - ${point.apCount} scan${point.apCount !== 1 ? 's' : ''}`}
              disabled={mode === 'remove' && !canDelete(point)}
            />
          ))}
        </div>
      )}

      {selectedPoint && (
        <div className="modal-overlay" onClick={() => setSelectedPoint(null)}>
          <div className="modal-content point-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Point #{selectedPoint.id}</h3>
              <button className="modal-close" onClick={() => setSelectedPoint(null)}>
                ×
              </button>
            </div>
            <div className="modal-body">
              <div className="point-detail-info">
                <p>
                  <strong>Coordinates:</strong> x={selectedPoint.longitude}, y=
                  {selectedPoint.latitude}
                </p>
                <p>
                  <strong>AP Count:</strong> {selectedPoint.apCount}
                </p>
              </div>
            </div>
            <div className="modal-footer">
              <button
                className="modal-button child-button"
                onClick={() => {
                  setSelectedPoint(null);
                  navigate(`/points/${selectedPoint.id}/scans`);
                }}
              >
                View Scans
              </button>
              {canDelete(selectedPoint) && (
                <button
                  className="modal-button delete-button"
                  onClick={() => {
                    setSelectedPoint(null);
                    setPointToDelete(selectedPoint);
                  }}
                >
                  Delete
                </button>
              )}
            </div>
          </div>
        </div>
      )}

      {pointToDelete && (
        <div className="modal-overlay" onClick={() => setPointToDelete(null)}>
          <div className="modal-content confirm-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>Confirm Delete</h3>
              <button className="modal-close" onClick={() => setPointToDelete(null)}>
                ×
              </button>
            </div>
            <div className="modal-body">
              <p>Are you sure you want to delete point #{pointToDelete.id}?</p>
              <p style={{ color: '#666', fontSize: '0.9rem', marginTop: '0.5rem' }}>
                Coordinates: x={pointToDelete.longitude}, y={pointToDelete.latitude}
              </p>
            </div>
            <div className="modal-footer">
              <button
                className="modal-button cancel-button"
                onClick={() => setPointToDelete(null)}
              >
                Cancel
              </button>
              <button className="modal-button delete-button" onClick={confirmDeletePoint}>
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
