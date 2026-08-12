import React, { useState, useEffect } from 'react';
import { Check, Save, AlertCircle } from 'lucide-react';
import { SettingsApi } from '../../api/settings.api';

const AdminSettings: React.FC = () => {
  const [settings, setSettings] = useState<any[]>([]);
  const [editedValues, setEditedValues] = useState<Record<string, string>>({});
  const [saving, setSaving] = useState<string | null>(null);
  const [savingAll, setSavingAll] = useState(false);
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const showMsg = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text });
    setTimeout(() => setMsg(null), 4000);
  };

  const loadSettings = async () => {
    try {
      const data = await SettingsApi.getAll();
      setSettings(data || []);
      // Initialize edited values from fetched data
      const values: Record<string, string> = {};
      (data || []).forEach((s: any) => { values[s.key] = s.value; });
      setEditedValues(values);
    } catch { }
  };

  useEffect(() => {
    loadSettings();
  }, []);

  const handleChange = (key: string, value: string) => {
    setEditedValues(prev => ({ ...prev, [key]: value }));
  };

  const isChanged = (key: string) => {
    const original = settings.find(s => s.key === key);
    return original && editedValues[key] !== original.value;
  };

  const hasAnyChanges = settings.some(s => editedValues[s.key] !== s.value);

  const handleSaveSingle = async (key: string) => {
    setSaving(key);
    try {
      await SettingsApi.update(key, { value: editedValues[key] });
      setSettings(prev => prev.map(s => s.key === key ? { ...s, value: editedValues[key] } : s));
      showMsg('success', `"${key.replace(/_/g, ' ')}" updated successfully`);
    } catch {
      showMsg('error', `Failed to update "${key.replace(/_/g, ' ')}"`);
    } finally {
      setSaving(null);
    }
  };

  const handleSaveAll = async () => {
    const changedKeys = settings.filter(s => editedValues[s.key] !== s.value).map(s => s.key);
    if (changedKeys.length === 0) return;

    setSavingAll(true);
    try {
      await Promise.all(
        changedKeys.map(key => SettingsApi.update(key, { value: editedValues[key] }))
      );
      setSettings(prev => prev.map(s =>
        changedKeys.includes(s.key) ? { ...s, value: editedValues[s.key] } : s
      ));
      showMsg('success', `All ${changedKeys.length} setting(s) saved successfully!`);
    } catch {
      showMsg('error', 'Failed to save some settings. Please try again.');
    } finally {
      setSavingAll(false);
    }
  };

  const grouped = settings.reduce((acc: Record<string, any[]>, curr: any) => {
    acc[curr.category] = acc[curr.category] || [];
    acc[curr.category].push(curr);
    return acc;
  }, {});

  return (
    <div className="space-y-6 max-w-4xl">
      {msg && (
        <div className={`p-4 rounded-xl flex items-center gap-3 ${msg.type === 'success' ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-red-50 text-red-700 border border-red-200'}`}>
          {msg.type === 'success' ? <Check className="w-5 h-5" /> : <AlertCircle className="w-5 h-5" />}
          <span className="text-sm font-medium">{msg.text}</span>
        </div>
      )}

      {Object.entries(grouped).map(([category, items]) => (
        <div key={category} className="card">
          <h2 className="text-xl font-bold text-gray-900 mb-6 border-b border-gray-100 pb-4">{category} Settings</h2>
          <div className="space-y-6">
            {items.map(setting => (
              <div key={setting.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div className="flex-1">
                  <p className="font-semibold text-gray-900 capitalize">{setting.key.replace(/_/g, ' ')}</p>
                  <p className="text-sm text-gray-500">{setting.description}</p>
                </div>
                <div className="flex items-center space-x-2">
                  <input
                    type="text"
                    value={editedValues[setting.key] ?? setting.value}
                    onChange={(e) => handleChange(setting.key, e.target.value)}
                    className={`input-field w-32 sm:w-48 py-2 ${isChanged(setting.key) ? 'border-amber-400 ring-1 ring-amber-200' : ''}`}
                  />
                  {isChanged(setting.key) && (
                    <button
                      onClick={() => handleSaveSingle(setting.key)}
                      disabled={saving === setting.key}
                      className="px-3 py-1.5 bg-primary-600 text-white text-xs font-semibold rounded-lg hover:bg-primary-700 transition-colors disabled:opacity-50 flex items-center gap-1"
                    >
                      <Save className="w-3.5 h-3.5" />
                      {saving === setting.key ? 'Saving...' : 'Save'}
                    </button>
                  )}
                  {saving === setting.key && !isChanged(setting.key) && (
                    <span className="text-xs text-primary-600">Saving...</span>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}

      {settings.length === 0 && (
        <div className="card text-center py-12 text-gray-500">Loading system settings...</div>
      )}

      {/* Save All Changes Button */}
      {settings.length > 0 && (
        <div className="flex justify-end pt-4 border-t border-gray-200">
          <button
            onClick={handleSaveAll}
            disabled={!hasAnyChanges || savingAll}
            className={`px-6 py-3 rounded-xl font-semibold text-sm flex items-center gap-2 transition-all ${
              hasAnyChanges
                ? 'bg-primary-600 text-white hover:bg-primary-700 shadow-md shadow-primary-500/20'
                : 'bg-gray-200 text-gray-400 cursor-not-allowed'
            }`}
          >
            <Save className="w-4 h-4" />
            {savingAll ? 'Saving All...' : 'Save All Changes'}
          </button>
        </div>
      )}
    </div>
  );
};

export default AdminSettings;

