import React, { useState, useEffect } from 'react';
import { Search, Activity } from 'lucide-react';
import { AuditLogApi } from '../../api/auditlog.api';

const AdminAuditLogs: React.FC = () => {
  const [logs, setLogs] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  const loadLogs = async () => {
    try {
      const data = await AuditLogApi.getAll({ pageSize: 50 });
      setLogs(data.items || []);
    } catch { } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadLogs();
  }, []);

  if (loading) return <div className="py-10 text-center">Loading audit logs...</div>;

  return (
    <div className="space-y-6">
      <div className="card">
        <h2 className="text-xl font-bold text-gray-900 mb-6 flex items-center space-x-2">
          <Activity className="w-5 h-5 text-primary-500" />
          <span>System Audit Trail</span>
        </h2>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200 bg-gray-50/50">
                <th className="text-left py-3 px-4 font-semibold text-gray-600 uppercase">Time</th>
                <th className="text-left py-3 px-4 font-semibold text-gray-600 uppercase">Actor</th>
                <th className="text-left py-3 px-4 font-semibold text-gray-600 uppercase">Action</th>
                <th className="text-left py-3 px-4 font-semibold text-gray-600 uppercase">Entity</th>
                <th className="text-left py-3 px-4 font-semibold text-gray-600 uppercase">Details</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((log) => (
                <tr key={log.id} className="border-b border-gray-50 hover:bg-gray-50/50 font-mono">
                  <td className="py-2.5 px-4 text-gray-500 whitespace-nowrap">
                    {new Date(log.performedAt).toLocaleString()}
                  </td>
                  <td className="py-2.5 px-4 font-semibold text-gray-900">{log.performedBy}</td>
                  <td className="py-2.5 px-4">
                    <span className={`px-2 py-0.5 rounded text-xs font-semibold ${
                      log.action.includes('Create') ? 'bg-green-100 text-green-700' :
                      log.action.includes('Delete') ? 'bg-red-100 text-red-700' :
                      log.action.includes('Update') ? 'bg-blue-100 text-blue-700' :
                      'bg-gray-100 text-gray-700'
                    }`}>
                      {log.action}
                    </span>
                  </td>
                  <td className="py-2.5 px-4 text-gray-700">{log.entityName}</td>
                  <td className="py-2.5 px-4 text-gray-500 truncate max-w-xs" title={log.details}>{log.details}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {logs.length === 0 && (
            <div className="py-12 text-center text-gray-500">No audit logs recorded yet.</div>
          )}
        </div>
      </div>
    </div>
  );
};

export default AdminAuditLogs;
