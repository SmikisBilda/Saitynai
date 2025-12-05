import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ViewModal } from './modals/ViewModal';
import './style/GenericTable.css';

interface Column<T> {
  key: keyof T | string;
  label: string;
  render?: (item: T) => React.ReactNode;
}

interface ChildRoute {
  label: string;
  path: (item: any) => string;
}

interface GenericTableProps<T> {
  title: string;
  data: T[];
  columns: Column<T>[];
  onEdit: (item: T) => void;
  onDelete: (item: T) => void;
  onCreate: () => void;
  childRoutes?: ChildRoute[];
  isLoading?: boolean;
  getItemId: (item: T) => number | string;
  canEdit?: (item: T) => boolean;
  canDelete?: (item: T) => boolean;
  canCreate?: boolean;
}

export function GenericTable<T>({
  title,
  data,
  columns,
  onEdit,
  onDelete,
  onCreate,
  childRoutes,
  isLoading = false,
  getItemId,
  canEdit,
  canDelete,
  canCreate = true,
}: GenericTableProps<T>) {
  const navigate = useNavigate();
  const [deleteConfirm, setDeleteConfirm] = useState<number | string | null>(null);
  const [viewItem, setViewItem] = useState<T | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 5;

  const totalPages = Math.ceil(data.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const paginatedData = data.slice(startIndex, endIndex);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleDelete = (item: T) => {
    const id = getItemId(item);
    if (deleteConfirm === id) {
      onDelete(item);
      setDeleteConfirm(null);
    } else {
      setDeleteConfirm(id);
      setTimeout(() => setDeleteConfirm(null), 3000);
    }
  };

  const getCellValue = (item: T, column: Column<T>) => {
    if (column.render) {
      return column.render(item);
    }
    const value = (item as any)[column.key];
    if (value === null || value === undefined) return '-';
    if (typeof value === 'boolean') return value ? 'Yes' : 'No';
    return String(value);
  };

  return (
    <div className="generic-table-container">
      <div className="table-header">
        <h2>{title}</h2>
        {canCreate && (
          <button className="create-button" onClick={onCreate}>
            + Create New
          </button>
        )}
      </div>

      {isLoading ? (
        <div className="loading">Loading...</div>
      ) : data.length === 0 ? (
        <div className="empty-state">No items found. Click "Create New" to add one.</div>
      ) : (
        <div className="table-wrapper">
          <table className="generic-table">
            <thead>
              <tr>
                {columns.map((column, index) => (
                  <th key={index}>{column.label}</th>
                ))}
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {paginatedData.map((item) => {
                const itemId = getItemId(item);
                const hasEditPermission = canEdit ? canEdit(item) : true;
                const hasDeletePermission = canDelete ? canDelete(item) : true;
                return (
                  <tr key={itemId}>
                    {columns.map((column, colIndex) => (
                      <td key={colIndex}>{getCellValue(item, column)}</td>
                    ))}
                    <td className="actions-cell">
                      <div className="action-buttons">
                        <button
                          className="action-button view-button"
                          onClick={() => setViewItem(item)}
                          title="View"
                        >
                          View
                        </button>
                        {hasEditPermission && (
                          <button
                            className="action-button edit-button"
                            onClick={() => onEdit(item)}
                            title="Edit"
                          >
                            Edit
                          </button>
                        )}
                        {hasDeletePermission && (
                          <button
                            className={`action-button delete-button ${
                              deleteConfirm === itemId ? 'confirm' : ''
                            }`}
                            onClick={() => handleDelete(item)}
                            title={deleteConfirm === itemId ? 'Click again to confirm' : 'Delete'}
                          >
                            {deleteConfirm === itemId ? 'Confirm?' : 'Delete'}
                          </button>
                        )}
                        {childRoutes && childRoutes.length > 0 && (
                          <div className="child-routes">
                            {childRoutes.map((route, idx) => (
                              <button
                                key={idx}
                                className="action-button child-button"
                                onClick={() => navigate(route.path(item))}
                                title={route.label}
                              >
                                {route.label}
                              </button>
                            ))}
                          </div>
                        )}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {!isLoading && data.length > itemsPerPage && (
        <div className="pagination">
          <button
            className="pagination-button"
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1}
          >
            Previous
          </button>
          <div className="pagination-info">
            Page {currentPage} of {totalPages}
          </div>
          <button
            className="pagination-button"
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
          >
            Next
          </button>
        </div>
      )}

      <ViewModal item={viewItem} onClose={() => setViewItem(null)} columns={columns as any} />
    </div>
  );
}
