import { useNavigate } from 'react-router-dom';
import { BackButton } from '../components/BackButton';
import './style/PermissionsPage.css';

const PermissionsPage = () => {
  const navigate = useNavigate();

  return (
    <div className="permissions-page">
      <div className="permissions-container">
        <div className="permissions-header">
          <BackButton label="Back" onClick={() => navigate('/')} />
          <h2>Permissions Management</h2>
        </div>
        
        <div className="permissions-buttons">
          <button onClick={() => navigate('/permissions/users')}>Users</button>
          <button onClick={() => navigate('/permissions/role-permissions')}>Role Permissions</button>
          <button onClick={() => navigate('/permissions/user-roles')}>User Roles</button>
        </div>
      </div>
    </div>
  );
};

export default PermissionsPage;
