import { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { GenericTable } from '../components/GenericTable';
import { EditModal } from '../components/modals/EditModal';
import { DeleteModal } from '../components/modals/DeleteModal';
import { BackButton } from '../components/BackButton';
import { getAccessPoints, deleteAccessPoint, updateAccessPoint, createAccessPoint } from '../services/accessPointService';
import { getScans } from '../services/scanService';
import { getPoints } from '../services/pointService';
import { getFloors } from '../services/floorService';
import { useAuth } from '../contexts/AuthContext';
import type { AccessPoint, CreateAccessPointDto, Scan, Point, Floor } from '../types/api';

const AccessPointsPage = () => {
  const { scanId } = useParams();
  const parsedScanId = Number(scanId);
  const navigate = useNavigate();

  const [accessPoints, setAccessPoints] = useState<AccessPoint[]>([]);
  const [scans, setScans] = useState<Scan[]>([]);
  const [points, setPoints] = useState<Point[]>([]);
  const [floors, setFloors] = useState<Floor[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [editItem, setEditItem] = useState<AccessPoint | null>(null);
  const [deleteItem, setDeleteItem] = useState<AccessPoint | null>(null);
  const [createMode, setCreateMode] = useState(false);

  const { permissionScopes, hasPermission, hasPermissionForId } = useAuth();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [apsResponse, scansResponse, pointsResponse, floorsResponse] = await Promise.all([
        getAccessPoints(),
        getScans(),
        getPoints(),
        getFloors()
      ]);
      const allAPs = apsResponse.data;
      const filtered = isNaN(parsedScanId)
        ? allAPs
        : allAPs.filter(ap => ap.scanId === parsedScanId);
      setAccessPoints(filtered);
      setScans(scansResponse.data);
      setPoints(pointsResponse.data);
      setFloors(floorsResponse.data);
    } catch (error) {
      console.error('Failed to load access points:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEdit = (ap: AccessPoint) => {
    setEditItem(ap);
  };

  const handleSaveEdit = async (updatedAP: AccessPoint) => {
    const updateDto = {
      scanId: updatedAP.scanId ?? undefined,
      ssid: updatedAP.ssid ?? undefined,
      bssid: updatedAP.bssid ?? undefined,
      capabilities: updatedAP.capabilities ?? undefined,
      centerfreq0: updatedAP.centerfreq0 ?? undefined,
      centerfreq1: updatedAP.centerfreq1 ?? undefined,
      frequency: updatedAP.frequency ?? undefined,
      level: updatedAP.level ?? undefined,
    };
    await updateAccessPoint(updatedAP.id, updateDto);
    setAccessPoints(accessPoints.map(ap => ap.id === updatedAP.id ? updatedAP : ap));
    setEditItem(null);
    setCreateMode(false);
  };

  const handleDelete = (ap: AccessPoint) => {
    setDeleteItem(ap);
  };

  const handleConfirmDelete = async () => {
    if (!deleteItem) return;
    await deleteAccessPoint(deleteItem.id);
    setAccessPoints(accessPoints.filter(ap => ap.id !== deleteItem.id));
    setDeleteItem(null);
  };

  const handleCreate = () => {
    setCreateMode(true);
    setEditItem({ id: 0, scanId: parsedScanId, ssid: '', bssid: '', capabilities: '', centerfreq0: null, centerfreq1: null, frequency: null, level: 0 } as AccessPoint);
  };

  const handleSaveCreate = async (newAP: AccessPoint) => {
    const createDto: CreateAccessPointDto = {
      scanId: newAP.scanId,
      ssid: newAP.ssid ?? undefined,
      bssid: newAP.bssid ?? undefined,
      capabilities: newAP.capabilities ?? undefined,
      centerfreq0: newAP.centerfreq0 ?? undefined,
      centerfreq1: newAP.centerfreq1 ?? undefined,
      frequency: newAP.frequency ?? undefined,
      level: newAP.level,
    };
    const response = await createAccessPoint(createDto);
    setAccessPoints([...accessPoints, response.data]);
    setEditItem(null);
    setCreateMode(false);
  };

  const columns = [
    { key: 'id', label: 'ID' },
    { key: 'scanId', label: 'Scan ID' },
    { key: 'ssid', label: 'SSID' },
    { key: 'bssid', label: 'BSSID' },
    { key: 'frequency', label: 'Frequency' },
    { key: 'level', label: 'Level' },
  ];

  // Build scan -> point -> floor -> building maps for cascade checks
  const scanToPointMap = useMemo(() => {
    const map: Record<number, number> = {};
    scans.forEach(s => {
      map[s.id] = s.pointId;
    });
    return map;
  }, [scans]);

  const pointToFloorMap = useMemo(() => {
    const map: Record<number, number> = {};
    points.forEach(p => {
      map[p.id] = p.floorId;
    });
    return map;
  }, [points]);

  const floorToBuildingMap = useMemo(() => {
    const map: Record<number, number> = {};
    floors.forEach(f => {
      map[f.id] = f.buildingId;
    });
    return map;
  }, [floors]);

  // Permission gating: direct or cascade from Scan, Point, Floor, or Building
  const apEditScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'edit' && s.resourceType === 'AccessPoint')
  ), [permissionScopes]);
  const apDeleteScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'delete' && s.resourceType === 'AccessPoint')
  ), [permissionScopes]);
  const apCreateScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'create' && s.resourceType === 'AccessPoint')
  ), [permissionScopes]);

  const cascadeScansForEdit = apEditScope?.cascadeFrom?.['Scan'] || [];
  const cascadeScansForDelete = apDeleteScope?.cascadeFrom?.['Scan'] || [];
  const cascadeScansForCreate = apCreateScope?.cascadeFrom?.['Scan'] || [];

  const cascadePointsForEdit = apEditScope?.cascadeFrom?.['Point'] || [];
  const cascadePointsForDelete = apDeleteScope?.cascadeFrom?.['Point'] || [];
  const cascadePointsForCreate = apCreateScope?.cascadeFrom?.['Point'] || [];

  const cascadeFloorsForEdit = apEditScope?.cascadeFrom?.['Floor'] || [];
  const cascadeFloorsForDelete = apDeleteScope?.cascadeFrom?.['Floor'] || [];
  const cascadeFloorsForCreate = apCreateScope?.cascadeFrom?.['Floor'] || [];

  const cascadeBuildingsForEdit = apEditScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForDelete = apDeleteScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForCreate = apCreateScope?.cascadeFrom?.['Building'] || [];

  const canEdit = (ap: AccessPoint) => {
    if (hasPermissionForId('edit', 'AccessPoint', ap.id)) return true;
    if (cascadeScansForEdit.includes(ap.scanId)) return true;
    const pointId = scanToPointMap[ap.scanId];
    if (pointId !== undefined && cascadePointsForEdit.includes(pointId)) return true;
    const floorId = pointToFloorMap[pointId];
    if (floorId !== undefined && cascadeFloorsForEdit.includes(floorId)) return true;
    const buildingId = floorToBuildingMap[floorId];
    return buildingId !== undefined && cascadeBuildingsForEdit.includes(buildingId);
  };

  const canDelete = (ap: AccessPoint) => {
    if (hasPermissionForId('delete', 'AccessPoint', ap.id)) return true;
    if (cascadeScansForDelete.includes(ap.scanId)) return true;
    const pointId = scanToPointMap[ap.scanId];
    if (pointId !== undefined && cascadePointsForDelete.includes(pointId)) return true;
    const floorId = pointToFloorMap[pointId];
    if (floorId !== undefined && cascadeFloorsForDelete.includes(floorId)) return true;
    const buildingId = floorToBuildingMap[floorId];
    return buildingId !== undefined && cascadeBuildingsForDelete.includes(buildingId);
  };

  const canCreate = () => {
    if (hasPermission('create', 'AccessPoint')) return true;
    if (!isNaN(parsedScanId) && cascadeScansForCreate.includes(parsedScanId)) return true;
    const pointId = scanToPointMap[parsedScanId];
    if (pointId !== undefined && cascadePointsForCreate.includes(pointId)) return true;
    const floorId = pointToFloorMap[pointId];
    if (floorId !== undefined && cascadeFloorsForCreate.includes(floorId)) return true;
    const buildingId = floorToBuildingMap[floorId];
    return buildingId !== undefined && cascadeBuildingsForCreate.includes(buildingId);
  };

  return (
    <>
      <BackButton 
        label="Back to Scans" 
        onClick={() => {
          const scan = scans.find(s => s.id === parsedScanId);
          if (scan) {
            const point = points.find(p => p.id === scan.pointId);
            if (point) {
              navigate(`/points/${point.id}/scans`);
              return;
            }
          }
          navigate('/');
        }} 
      />
      <GenericTable
        title={isNaN(parsedScanId) ? 'Access Points' : `Access Points of Scan #${parsedScanId}`}
        data={accessPoints}
        columns={columns}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        isLoading={isLoading}
        getItemId={(ap) => ap.id}
        canEdit={canEdit}
        canDelete={canDelete}
        canCreate={canCreate()}
      />

      <EditModal
        item={editItem}
        onSave={createMode ? handleSaveCreate : handleSaveEdit}
        onClose={() => {
          setEditItem(null);
          setCreateMode(false);
        }}
        title={createMode ? 'Create Access Point' : 'Edit Access Point'}
      />

      <DeleteModal
        item={deleteItem}
        onConfirm={handleConfirmDelete}
        onClose={() => setDeleteItem(null)}
        itemName="access point"
        getDisplayText={(ap) => `${ap.ssid || ap.bssid || `Access Point #${ap.id}`} (Scan ${ap.scanId})`}
      />
    </>
  );
};

export default AccessPointsPage;
