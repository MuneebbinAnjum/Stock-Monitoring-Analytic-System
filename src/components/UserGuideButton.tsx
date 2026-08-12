import React, { useState } from 'react'
import { Info, X } from 'lucide-react'

const UserGuideButton: React.FC = () => {
  const [open, setOpen] = useState(false)
  const [tab, setTab] = useState<'Admin' | 'Salesman' | 'Buyer'>('Admin')

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        title="User Guide"
        className="fixed bottom-6 right-6 z-50 bg-primary-600 hover:bg-primary-700 text-white rounded-full p-3 shadow-lg flex items-center gap-2"
      >
        <Info className="w-4 h-4" />
        <span className="hidden sm:inline text-sm font-medium">User Guide</span>
      </button>

      {open && (
        <div className="fixed inset-0 z-60 flex items-center justify-center p-4 bg-black/50">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-3xl max-h-[80vh] overflow-y-auto">
            <div className="flex items-center justify-between p-4 border-b">
              <h3 className="text-lg font-bold">User Guide</h3>
              <button onClick={() => setOpen(false)} className="text-gray-500 hover:text-gray-700">
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="p-4">
              <div className="flex gap-2 mb-4">
                {(['Admin', 'Salesman', 'Buyer'] as const).map(r => (
                  <button
                    key={r}
                    onClick={() => setTab(r)}
                    className={`px-3 py-1 rounded-md text-sm ${tab === r ? 'bg-primary-600 text-white' : 'bg-gray-100 text-gray-700'}`}
                  >
                    {r}
                  </button>
                ))}
              </div>

              <div className="prose text-sm text-gray-700">
                {tab === 'Admin' && (
                  <div>
                    <h4 className="font-semibold">Admin Portal</h4>
                    <ul>
                      <li>Manage products: add, edit, set tax %, and set discounts for periods via the Products section.</li>
                      <li>Approve or reject salesman accounts from Employees  Pending Approvals.</li>
                      <li>Set monthly salary for salesmen and manage per-product commission in Employees  Manage Commissions.</li>
                      <li>View orders, approve/reject, dispatch orders and see order history in Orders.</li>
                      <li>View all notifications and notification history in Notifications. All admin notifications are visible here.</li>
                      <li>Complaints can be viewed and responded to under Complaints.</li>
                    </ul>
                  </div>
                )}

                {tab === 'Salesman' && (
                  <div>
                    <h4 className="font-semibold">Salesman Portal</h4>
                    <ul>
                      <li>After admin approval you can log in and access your dashboard.</li>
                      <li>You can view assigned products, create orders on behalf of buyers, and view your commission summary.</li>
                      <li>Add to cart is not available for salesmen. Use the Salesman order screens to create sales.</li>
                      <li>Notifications addressed to you will appear in your notifications area.</li>
                    </ul>
                  </div>
                )}

                {tab === 'Buyer' && (
                  <div>
                    <h4 className="font-semibold">Buyer Portal</h4>
                    <ul>
                      <li>Browse products, add items to cart and checkout from the Cart/Checkout pages.</li>
                      <li>View your orders and track delivery via Order Tracking.</li>
                      <li>Raise complaints from the Complaints page and view responses from admin.</li>
                      <li>You can change your password from Change Password.</li>
                    </ul>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  )
}

export default UserGuideButton
