export interface User {
  id: string;
  email: string;
  fullName: string;
  role: 'Admin' | 'Salesman' | 'Buyer';
  approvalStatus?: 'Pending' | 'Approved' | 'Rejected';
  phone?: string;
}

export interface Product {
  id: string;
  productId?: string;
  name: string;
  sku: string;
  description?: string;
  categoryId: string;
  brandName?: string;
  companyName?: string;
  model?: string;
  unitPrice: number;
  purchasePrice?: number;
  discountPrice?: number;
  stockQuantity: number;
  reorderLevel: number;
  deliveryPeriod: string;
  supplierId?: string;
  supplierName?: string;
  categoryName?: string;
  viewCount?: number;
  warrantyInfo?: string;
  weight?: string;
  dimensions?: string;
  taxPercentage?: number;
  tags?: string;
  productImages?: ProductImage[];
  createdAt: string;
  updatedAt: string;
}

export interface ProductImage {
  id?: string;
  imageId?: string;
  productId: string;
  imageUrl: string;
  altText?: string;
  displayOrder: number;
}

export interface Order {
  orderId: string;
  orderNumber: string;
  customerId: string;
  employeeId?: string;
  orderType: 'Online' | 'Physical';
  orderDate: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Dispatched' | 'Delivered' | 'Cancelled' | 'Received';
  totalAmount: number;
  deliveryCity?: string;
  deliveryAddress?: string;
  deliveryPeriod?: string;
  paymentMethod: string;
  orderItems: OrderItem[];
  createdAt: string;
  updatedAt: string;
}

export interface OrderItem {
  orderItemId: string;
  orderId: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  product?: Product;
}

export interface OrderItemResponse {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface OrderResponse {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  employeeId?: string;
  employeeName?: string;
  orderDate: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Dispatched' | 'Delivered' | 'Cancelled' | 'Received';
  totalAmount: number;
  taxAmount?: number;
  discountAmount?: number;
  deliveryCity: string;
  deliveryAddress: string;
  deliveryPeriod: string;
  paymentMethod: string;
  courierRef: string;
  items: OrderItemResponse[];
  createdAt: string;
  updatedAt: string;
}

export interface Complaint {
  complaintId: string;
  orderId: string;
  customerId: string;
  complaintType: 'Product' | 'Delivery' | 'Return Request';
  title: string;
  description: string;
  status: 'Open' | 'In Review' | 'Resolved';
  adminNotes?: string;
  returnApproved?: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Notification {
  notificationId: string;
  employeeId?: string;
  title: string;
  message: string;
  notificationType: string;
  relatedId?: string;
  isRead: boolean;
  createdAt: string;
}

export interface Category {
  categoryId: string;
  name: string;
  description?: string;
  createdAt: string;
}
