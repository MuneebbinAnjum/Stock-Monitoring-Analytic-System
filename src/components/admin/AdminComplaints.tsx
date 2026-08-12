import React, { useState, useEffect } from 'react';
import { Search, AlertCircle, File, CheckCircle } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { ComplaintApi } from '../../api/complaint.api';

const AdminComplaints: React.FC = () => {
  const [complaints, setComplaints] = useState<any[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [conversationOpen, setConversationOpen] = useState(false);
  const [activeComplaint, setActiveComplaint] = useState<any | null>(null);
  const [messages, setMessages] = useState<any[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [convoLoading, setConvoLoading] = useState(false);

  const showMsg = (type: 'success' | 'error', text: string) => {
    setMsg({ type, text });
    setTimeout(() => setMsg(null), 4000);
  };

  const loadComplaints = async () => {
    try {
      const data = await ComplaintApi.getAll();
      setComplaints(data || []);
    } catch (err: any) { console.error('FAILED TO GET COMPLAINTS:', err); alert('Error: ' + err.message); } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadComplaints();

    const handleNotification = (e: any) => {
      const type = e.detail?.notificationType;
      if (type === 'NewComplaint' || type === 'ComplaintResponse' || type === 'ComplaintReply' || type === 'ComplaintMessage') {
        loadComplaints();
        // If a conversation is currently open and it matches the notification's related ID, we might want to refresh it
        // but for now, just reloading the complaints list will show updated status.
        if (activeComplaint && (e.detail?.relatedId === activeComplaint.id || e.detail?.complaintId === activeComplaint.id)) {
          handleOpenConversation(activeComplaint.id);
        }
      }
    };

    window.addEventListener('NotificationReceived', handleNotification);
    return () => window.removeEventListener('NotificationReceived', handleNotification);
  }, [activeComplaint]);

  const handleUpdateStatus = async (id: string, newStatus: string) => {
    try {
      const complaint = complaints.find(c => c.id === id);
      const isReturn = complaint?.complaintType === 'Return Request';
      const payload: any = { status: newStatus, complaintType: complaint?.complaintType };
      
      // Auto-approve or reject returns based on status resolution
      if (isReturn && newStatus === 'Resolved') payload.returnApproved = true;
      if (isReturn && newStatus === 'Rejected') payload.returnApproved = false;

      await ComplaintApi.updateStatus(id, payload);
      showMsg('success', `Complaint marked as ${newStatus}`);
      loadComplaints();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to update complaint status');
    }
  };

  const handleOpenConversation = async (id: string) => {
    setConvoLoading(true);
    try {
      const data = await ComplaintApi.getById(id);
      setActiveComplaint(data);
      setMessages(data.messages || []);
      setConversationOpen(true);
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to load conversation');
    } finally {
      setConvoLoading(false);
    }
  };

  const handleSendMessage = async () => {
    if (!activeComplaint) return;
    if (!newMessage.trim()) return;
    try {
      const sent = await ComplaintApi.postMessage(activeComplaint.id, { message: newMessage.trim() });
      setMessages(prev => [...prev, sent]);
      setNewMessage('');
      showMsg('success', 'Reply sent');
      loadComplaints();
    } catch (err: any) {
      showMsg('error', err.response?.data?.message || 'Failed to send message');
    }
  };

  const filteredComplaints = complaints.filter(c => 
    c.title?.toLowerCase().includes(searchTerm.toLowerCase()) || 
    c.orderNumber?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <>
    <div className="space-y-6">
      <AnimatePresence>
        {msg && (
          <motion.div
            initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0 }}
            className={`p-4 rounded-xl flex items-center gap-3 ${msg.type === 'success'
              ? 'bg-emerald-50 border border-emerald-200 text-emerald-700'
              : 'bg-red-50 border border-red-200 text-red-700'}`}
          >
            {msg.type === 'success' ? <CheckCircle className="w-5 h-5" /> : <AlertCircle className="w-5 h-5" />}
            <span className="text-sm font-medium">{msg.text}</span>
          </motion.div>
        )}
      </AnimatePresence>

      <div className="card">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-2.5 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Search by order or title..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input-field pl-10"
            />
          </div>
        </div>

        {loading ? (
          <div className="py-10 text-center text-gray-500">Loading complaints...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-gray-200 bg-gray-50/50">
                  <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Complaint Info</th>
                  <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Order / Customer</th>
                  <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Type</th>
                  <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Status</th>
                  <th className="text-left py-3 px-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredComplaints.map((c) => (
                  <tr key={c.id} className="border-b border-gray-50 hover:bg-gray-50/50 transition-colors">
                    <td className="py-3 px-4">
                      <p className="font-semibold text-gray-900 text-sm">{c.title}</p>
                      <p className="text-xs text-gray-500 max-w-[250px] truncate" title={c.description}>{c.description}</p>
                    </td>
                    <td className="py-3 px-4">
                      <p className="font-mono font-medium text-primary-600 text-sm">{c.orderNumber}</p>
                      <p className="text-xs text-gray-500">{c.customerName}</p>
                    </td>
                    <td className="py-3 px-4">
                      <span className={`px-2.5 py-1 rounded-full text-[10px] font-bold ${
                        c.complaintType === 'Return Request' ? 'bg-purple-100 text-purple-700' : 'bg-gray-100 text-gray-700'
                      }`}>
                        {c.complaintType}
                      </span>
                    </td>
                    <td className="py-3 px-4">
                      <span className={`px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider ${
                        c.status === 'Resolved' ? 'bg-green-100 text-green-700' :
                        c.status === 'Rejected' ? 'bg-red-100 text-red-700' :
                        c.status === 'In Review' ? 'bg-blue-100 text-blue-700' :
                        'bg-amber-100 text-amber-700'
                      }`}>
                        {c.status}
                      </span>
                    </td>
                    <td className="py-3 px-4">
                      <div className="flex items-center space-x-2">
                        <select
                          value={c.status}
                          onChange={(e) => handleUpdateStatus(c.id, e.target.value)}
                          className="text-xs font-semibold border border-gray-300 rounded-lg px-2 py-1.5 focus:ring-primary-500 bg-white shadow-sm cursor-pointer"
                          disabled={c.status === 'Resolved' || c.status === 'Rejected'}
                        >
                          <option value="Open">Open</option>
                          <option value="In Review">In Review</option>
                          <option value="Resolved">Resolve / Approve</option>
                          <option value="Rejected">Reject</option>
                        </select>
                        <button
                          onClick={() => handleOpenConversation(c.id)}
                          className="text-xs px-2 py-1 bg-primary-600 text-white rounded-md"
                        >
                          Reply
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {filteredComplaints.length === 0 && (
              <div className="py-12 flex flex-col items-center justify-center text-gray-400">
                <File className="w-12 h-12 mb-3 text-gray-300" />
                <p>No complaints found.</p>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
    {/* Conversation Modal rendered after card to avoid JSX nesting issues */}
    {conversationOpen && activeComplaint && (
      <div className="fixed inset-0 z-50 flex items-start justify-center p-6">
        <div className="absolute inset-0 bg-black/40" onClick={() => { setConversationOpen(false); setActiveComplaint(null); setMessages([]); }} />
        <div className="bg-white rounded-2xl shadow-xl max-w-2xl w-full p-6 relative z-10">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-lg font-bold">Conversation - {activeComplaint.title}</h3>
            <button className="text-sm text-gray-500" onClick={() => { setConversationOpen(false); setActiveComplaint(null); setMessages([]); }}>Close</button>
          </div>

          <div className="space-y-3 max-h-[60vh] overflow-y-auto mb-4">
            {messages.length === 0 && (
              <div className="text-sm text-gray-500">No messages yet.</div>
            )}
            {messages.map((m: any) => (
              <div key={m.id} className={`p-3 rounded-xl ${m.senderType === 'Employee' ? 'bg-primary-50 self-end' : 'bg-gray-100'}`}>
                <div className="flex justify-between items-start">
                  <div>
                    <div className="text-sm font-semibold">{m.senderType === 'Employee' ? 'Admin' : activeComplaint.customerName}</div>
                    <div className="text-sm text-gray-700">{m.message}</div>
                  </div>
                  <div className="text-xs text-gray-400">{new Date(m.createdAt).toLocaleString()}</div>
                </div>
              </div>
            ))}
          </div>

          <div className="flex items-center gap-3">
            <input value={newMessage} onChange={(e) => setNewMessage(e.target.value)} placeholder="Write a reply..." className="input-field flex-1" />
            <button onClick={handleSendMessage} className="px-4 py-2 bg-primary-600 text-white rounded-md">Send</button>
          </div>
        </div>
      </div>
    )}
    </>
  );
};

export default AdminComplaints;
