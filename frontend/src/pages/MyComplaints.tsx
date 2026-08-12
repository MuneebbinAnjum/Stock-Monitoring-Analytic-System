import React, { useEffect, useState } from 'react'
import { ComplaintApi } from '../api/complaint.api'
import { motion, AnimatePresence } from 'framer-motion'
import { Plus, X } from 'lucide-react'

const MyComplaints: React.FC = () => {
  const [complaints, setComplaints] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [conversationOpen, setConversationOpen] = useState(false)
  const [activeComplaint, setActiveComplaint] = useState<any | null>(null)
  const [messages, setMessages] = useState<any[]>([])
  const [newMessage, setNewMessage] = useState('')
  
  // Create Modal State
  const [createModalOpen, setCreateModalOpen] = useState(false)
  const [newComplaint, setNewComplaint] = useState({
    title: '',
    description: '',
    orderNumber: '',
    complaintType: 'General'
  })
  const [submitting, setSubmitting] = useState(false)
  const [errorMsg, setErrorMsg] = useState('')

  const load = async () => {
    setLoading(true)
    try {
      const data = await ComplaintApi.getMyComplaints()
      setComplaints(data || [])
    } catch { }
    setLoading(false)
  }

  useEffect(() => { 
    load() 

    const handleNotification = (e: any) => {
      const type = e.detail?.notificationType;
      if (type === 'ComplaintResponse' || type === 'ComplaintReply' || type === 'ComplaintMessage') {
        load();
        if (activeComplaint && (e.detail?.relatedId === activeComplaint.id || e.detail?.complaintId === activeComplaint.id)) {
          openConversation(activeComplaint.id);
        }
      }
    };

    window.addEventListener('NotificationReceived', handleNotification);
    return () => window.removeEventListener('NotificationReceived', handleNotification);
  }, [activeComplaint])

  const openConversation = async (id: string) => {
    try {
      const data = await ComplaintApi.getById(id)
      setActiveComplaint(data)
      setMessages(data.messages || [])
      setConversationOpen(true)
    } catch { }
  }

  const sendMessage = async () => {
    if (!activeComplaint || !newMessage.trim()) return
    try {
      const sent = await ComplaintApi.postMessage(activeComplaint.id, { message: newMessage.trim() })
      setMessages(prev => [...prev, sent])
      setNewMessage('')
      load()
    } catch { }
  }

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!newComplaint.title.trim() || !newComplaint.description.trim()) {
      setErrorMsg('Title and description are required')
      return
    }
    setSubmitting(true)
    setErrorMsg('')
    try {
      await ComplaintApi.create(newComplaint)
      setCreateModalOpen(false)
      setNewComplaint({ title: '', description: '', orderNumber: '', complaintType: 'General' })
      load()
    } catch (err: any) {
      setErrorMsg(err.response?.data?.message || 'Failed to create complaint')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page-container py-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold text-gray-900">My Complaints</h1>
          <button 
            onClick={() => setCreateModalOpen(true)}
            className="btn-primary flex items-center space-x-2 py-2 px-4"
          >
            <Plus className="w-4 h-4" />
            <span>New Complaint</span>
          </button>
        </div>

        <div className="card">
          {loading ? (
            <div className="py-8 text-center text-gray-500">Loading...</div>
          ) : complaints.length === 0 ? (
            <div className="py-12 text-center text-gray-500">
              <p>You have no complaints yet.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {complaints.map(c => (
                <div key={c.id} className="p-4 rounded-xl border border-gray-100 hover:border-gray-200 transition-colors bg-white shadow-sm">
                  <div className="flex justify-between items-start">
                    <div>
                      <div className="font-bold text-gray-900 flex items-center gap-2">
                        {c.title}
                        <span className="px-2 py-0.5 rounded-full bg-gray-100 text-gray-600 text-[10px] uppercase font-bold tracking-wider">
                          {c.complaintType}
                        </span>
                      </div>
                      <div className="text-sm text-gray-600 mt-1 line-clamp-2">{c.description}</div>
                      {c.orderNumber && (
                        <div className="text-xs text-gray-500 mt-2 font-mono">Order: {c.orderNumber}</div>
                      )}
                    </div>
                    <div className="text-xs text-gray-400 whitespace-nowrap">{new Date(c.createdAt).toLocaleDateString()}</div>
                  </div>
                  <div className="mt-4 flex items-center justify-between border-t border-gray-50 pt-3">
                    <div className="flex items-center gap-2">
                      <span className="text-xs text-gray-500">Status:</span>
                      <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider ${
                        c.status === 'Resolved' ? 'bg-emerald-100 text-emerald-700' :
                        c.status === 'Rejected' ? 'bg-red-100 text-red-700' :
                        c.status === 'In Review' ? 'bg-blue-100 text-blue-700' :
                        'bg-amber-100 text-amber-700'
                      }`}>
                        {c.status}
                      </span>
                    </div>
                    <button onClick={() => openConversation(c.id)} className="px-4 py-1.5 bg-primary-50 text-primary-600 font-semibold rounded-lg text-xs hover:bg-primary-100 transition-colors">
                      View / Reply
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Conversation Modal */}
      <AnimatePresence>
        {conversationOpen && activeComplaint && (
          <div className="fixed inset-0 z-50 flex items-start justify-center p-4 sm:p-6 pt-20">
            <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={() => { setConversationOpen(false); setActiveComplaint(null); setMessages([]); }} />
            <motion.div initial={{ scale: 0.95, opacity: 0 }} animate={{ scale: 1, opacity: 1 }} exit={{ scale: 0.95, opacity: 0 }} className="bg-white rounded-2xl shadow-xl max-w-2xl w-full p-6 relative z-10 flex flex-col max-h-[85vh]">
              <div className="flex justify-between items-center mb-4 pb-3 border-b border-gray-100 flex-shrink-0">
                <div>
                  <h3 className="text-lg font-bold text-gray-900">{activeComplaint.title}</h3>
                  <p className="text-xs text-gray-500 mt-1">Status: {activeComplaint.status}</p>
                </div>
                <button className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-full transition-colors" onClick={() => { setConversationOpen(false); setActiveComplaint(null); setMessages([]); }}>
                  <X className="w-5 h-5" />
                </button>
              </div>

              <div className="space-y-4 overflow-y-auto pr-2 flex-1 mb-4 custom-scrollbar">
                {messages.length === 0 && (
                  <div className="text-sm text-gray-400 text-center py-8">No messages yet. Start the conversation!</div>
                )}
                {messages.map((m: any) => (
                  <div key={m.id} className={`flex ${m.senderType === 'Buyer' ? 'justify-end' : 'justify-start'}`}>
                    <div className={`p-3.5 rounded-2xl max-w-[85%] ${m.senderType === 'Buyer' ? 'bg-primary-600 text-white rounded-tr-sm' : 'bg-gray-100 text-gray-800 rounded-tl-sm'}`}>
                      <div className="text-[11px] font-bold opacity-70 mb-1">{m.senderType === 'Buyer' ? 'You' : 'Admin Support'}</div>
                      <div className="text-sm">{m.message}</div>
                      <div className="text-[10px] opacity-60 mt-2 text-right">{new Date(m.createdAt).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</div>
                    </div>
                  </div>
                ))}
              </div>

              <div className="flex items-center gap-2 pt-3 border-t border-gray-100 flex-shrink-0">
                <input 
                  value={newMessage} 
                  onChange={(e) => setNewMessage(e.target.value)} 
                  onKeyDown={(e) => e.key === 'Enter' && sendMessage()}
                  placeholder="Type your message..." 
                  className="input-field flex-1 bg-gray-50" 
                  disabled={activeComplaint.status === 'Resolved' || activeComplaint.status === 'Rejected'}
                />
                <button 
                  onClick={sendMessage} 
                  disabled={!newMessage.trim() || activeComplaint.status === 'Resolved' || activeComplaint.status === 'Rejected'}
                  className="px-6 py-2.5 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-xl transition-colors disabled:opacity-50"
                >
                  Send
                </button>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>

      {/* Create Complaint Modal */}
      <AnimatePresence>
        {createModalOpen && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }} className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={() => setCreateModalOpen(false)} />
            <motion.div initial={{ scale: 0.95, opacity: 0 }} animate={{ scale: 1, opacity: 1 }} exit={{ scale: 0.95, opacity: 0 }} className="bg-white rounded-2xl shadow-xl max-w-md w-full p-6 relative z-10">
              <div className="flex justify-between items-center mb-6">
                <h3 className="text-xl font-bold text-gray-900">Create New Complaint</h3>
                <button onClick={() => setCreateModalOpen(false)} className="text-gray-400 hover:text-gray-600">
                  <X className="w-5 h-5" />
                </button>
              </div>

              {errorMsg && (
                <div className="mb-4 p-3 bg-red-50 text-red-600 text-sm rounded-lg border border-red-100">
                  {errorMsg}
                </div>
              )}

              <form onSubmit={handleCreate} className="space-y-4">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-1">Title *</label>
                  <input
                    type="text"
                    required
                    value={newComplaint.title}
                    onChange={e => setNewComplaint({...newComplaint, title: e.target.value})}
                    className="input-field"
                    placeholder="Brief summary of issue"
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-1">Type *</label>
                  <select
                    value={newComplaint.complaintType}
                    onChange={e => setNewComplaint({...newComplaint, complaintType: e.target.value})}
                    className="input-field bg-white"
                  >
                    <option value="General">General Inquiry</option>
                    <option value="Damage">Damaged Product</option>
                    <option value="Delay">Delivery Delay</option>
                    <option value="Return Request">Return Request</option>
                    <option value="Other">Other</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-1">Order Number (Optional)</label>
                  <input
                    type="text"
                    value={newComplaint.orderNumber}
                    onChange={e => setNewComplaint({...newComplaint, orderNumber: e.target.value})}
                    className="input-field"
                    placeholder="e.g. ORD-12345"
                  />
                </div>

                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-1">Description *</label>
                  <textarea
                    required
                    rows={4}
                    value={newComplaint.description}
                    onChange={e => setNewComplaint({...newComplaint, description: e.target.value})}
                    className="input-field resize-none"
                    placeholder="Please provide details about your issue..."
                  />
                </div>

                <div className="pt-4">
                  <button
                    type="submit"
                    disabled={submitting}
                    className="btn-primary w-full py-3 flex justify-center items-center"
                  >
                    {submitting ? (
                      <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    ) : (
                      "Submit Complaint"
                    )}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  )
}

export default MyComplaints
