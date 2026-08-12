import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Users, CheckCircle, ShoppingCart, DollarSign, Package, TrendingUp, Activity, MapPin } from 'lucide-react';
import { LineChart, Line, BarChart, Bar, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, AreaChart, Area } from 'recharts';
import api from '../../api/client';
import { ReportApi } from '../../api/report.api';

interface AdminOverviewProps {
  orders: any[];
  pendingSalesman: any[];
  products: any[];
}

const AdminOverview: React.FC<AdminOverviewProps> = ({ orders, pendingSalesman, products }) => {
  const [notifCount, setNotifCount] = useState(0);
  const [revenueChartData, setRevenueChartData] = useState<any[]>([]);
  const [salesByCity, setSalesByCity] = useState<any[]>([]);
  const [employeeRevenue, setEmployeeRevenue] = useState<any[]>([]);
  const [productRevenue, setProductRevenue] = useState<any[]>([]);
  const [isLoadingCharts, setIsLoadingCharts] = useState(false);
  const [cityViewMode, setCityViewMode] = useState<'count' | 'revenue'>('count');

  // Agent Earnings State
  const [salesmenList, setSalesmenList] = useState<any[]>([]);
  const [selectedSalesmanId, setSelectedSalesmanId] = useState<string>('');
  const [agentDays, setAgentDays] = useState<number>(7);
  const [agentEarningsData, setAgentEarningsData] = useState<any>(null);
  const [isLoadingEarnings, setIsLoadingEarnings] = useState(false);

  const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#14b8a6', '#f97316'];

  useEffect(() => {
    api.get('/notifications/count').then(r => setNotifCount(r.data.data || 0)).catch(() => { });
    api.get('/Employees').then(r => {
      const salesmen = (r.data.data || []).filter((e: any) => e.role === 'Salesman');
      setSalesmenList(salesmen);
      if (salesmen.length > 0) setSelectedSalesmanId(salesmen[0].id);
    }).catch(() => { });
  }, []);

  useEffect(() => {
    if (selectedSalesmanId) {
      fetchAgentEarnings();
    }
  }, [selectedSalesmanId, agentDays]);

  const fetchAgentEarnings = async () => {
    setIsLoadingEarnings(true);
    try {
      const data = await ReportApi.getAgentEarnings(selectedSalesmanId, agentDays);
      setAgentEarningsData(data);
    } catch (err) {
      console.error('Error fetching agent earnings:', err);
    } finally {
      setIsLoadingEarnings(false);
    }
  };

  useEffect(() => {
    loadChartData();
  }, [orders]);

  const loadChartData = async () => {
    setIsLoadingCharts(true);
    try {
      // Fetch live data from API endpoints
      const thirtyDaysAgo = new Date();
      thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
      
      const [salesSummary, locationData, employeeData, categoryData] = await Promise.all([
        ReportApi.getSalesSummary(thirtyDaysAgo, new Date()).catch(() => null),
        ReportApi.getSalesByLocation().catch(() => null),
        ReportApi.getRevenueBreakdown('employee').catch(() => null),
        ReportApi.getRevenueBreakdown('category').catch(() => null)
      ]);

      // Process sales summary for revenue trend
      if (salesSummary?.dailyRevenue) {
        const chartData = Object.entries(salesSummary.dailyRevenue)
          .map(([date, revenue]) => ({ date, revenue }))
          .sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
        setRevenueChartData(chartData);
      }

      // Process sales by location — map API shape { groupName, revenue, percentage } to chart shape { city, count, revenue }
      if (locationData && Array.isArray(locationData)) {
        const mapped = locationData.map((item: any) => ({
          city: item.groupName ?? item.city ?? 'Unknown',
          revenue: item.revenue ?? 0,
          count: item.count ?? Math.round(item.percentage ?? 0), // derive order count from percentage if count not present
        }));
        setSalesByCity(mapped.slice(0, 10));
      }

      // Process employee revenue — map API shape { groupName, revenue } to chart shape { name, revenue }
      if (employeeData && Array.isArray(employeeData)) {
        const mapped = employeeData.map((item: any) => ({
          name: item.groupName ?? item.name ?? 'Unknown',
          revenue: item.revenue ?? 0,
        }));
        setEmployeeRevenue(mapped.slice(0, 10));
      }

      // Fallback to local calculation if API fails
      if (!salesSummary || !locationData) {
        prepareChartDataLocal();
      }
    } catch (err) {
      console.error('Error loading chart data:', err);
      prepareChartDataLocal();
    } finally {
      setIsLoadingCharts(false);
    }
  };

  const prepareChartDataLocal = () => {
    // Prepare daily revenue data for line chart
    const dailyData: Record<string, number> = {};
    const cityData: Record<string, { count: number; revenue: number }> = {};
    const empData: Record<string, { name: string; revenue: number }> = {};
    const prodData: Record<string, { name: string; revenue: number }> = {};

    orders.forEach(order => {
      const date = new Date(order.orderDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      const city = order.deliveryCity || 'Unknown';
      
      if (['Received', 'Delivered', 'Dispatched', 'Approved'].includes(order.status)) {
        dailyData[date] = (dailyData[date] || 0) + order.totalAmount;
        
        // City breakdown
        cityData[city] = cityData[city] || { count: 0, revenue: 0 };
        cityData[city].count += 1;
        cityData[city].revenue += order.totalAmount;

        // Employee revenue
        if (order.employeeName) {
          empData[order.employeeName] = empData[order.employeeName] || { name: order.employeeName, revenue: 0 };
          empData[order.employeeName].revenue += order.totalAmount;
        }
      }
      
      // Product revenue (from order items)
      (order.items || []).forEach((item: any) => {
        const prodKey = item.productId;
        prodData[prodKey] = prodData[prodKey] || { name: item.productName, revenue: 0 };
        prodData[prodKey].revenue += item.quantity * item.unitPrice;
      });
    });

    setRevenueChartData(
      Object.entries(dailyData)
        .map(([date, revenue]) => ({ date, revenue }))
        .slice(-30) // Last 30 days
    );

    setSalesByCity(
      Object.entries(cityData)
        .map(([city, data]) => ({ city, ...data }))
        .sort((a, b) => b.revenue - a.revenue)
    );

    setEmployeeRevenue(
      Object.values(empData)
        .sort((a: any, b: any) => b.revenue - a.revenue)
        .slice(0, 10)
    );

    setProductRevenue(
      Object.values(prodData)
        .sort((a: any, b: any) => b.revenue - a.revenue)
        .slice(0, 6)
    );
  };

  // Real calculations from actual order data
  const completedOrders = orders.filter(o => ['Received', 'Delivered', 'Dispatched', 'Approved'].includes(o.status));
  const totalRevenue = completedOrders.reduce((sum, o) => sum + o.totalAmount, 0);
  const pendingOrders = orders.filter(o => o.status === 'Pending').length;
  const rejectedOrders = orders.filter(o => o.status === 'Rejected' || o.status === 'Cancelled').length;
  const todayOrders = orders.filter(o => new Date(o.orderDate).toDateString() === new Date().toDateString());
  const todayRevenue = todayOrders
    .filter(o => ['Received', 'Delivered', 'Dispatched', 'Approved'].includes(o.status))
    .reduce((sum, o) => sum + o.totalAmount, 0);

  const lowStockProducts = products.filter(p => p.stockQuantity > 0 && p.stockQuantity <= p.reorderLevel);
  const outOfStockProducts = products.filter(p => p.stockQuantity === 0);

  // Order status breakdown
  const statusBreakdown = ['Pending', 'Approved', 'Dispatched', 'Delivered', 'Received', 'Rejected', 'Cancelled']
    .map(status => ({
      status,
      count: orders.filter(o => o.status === status).length,
      pct: orders.length > 0 ? Math.round((orders.filter(o => o.status === status).length / orders.length) * 100) : 0
    }))
    .filter(s => s.count > 0);

  // Top products by order count
  const productFreq: Record<string, { name: string; count: number; revenue: number }> = {};
  orders.forEach(order => {
    (order.items || []).forEach((item: any) => {
      if (!productFreq[item.productId]) {
        productFreq[item.productId] = { name: item.productName, count: 0, revenue: 0 };
      }
      productFreq[item.productId].count += item.quantity;
      productFreq[item.productId].revenue += item.quantity * item.unitPrice;
    });
  });
  const topProducts = Object.values(productFreq).sort((a, b) => b.count - a.count).slice(0, 5);

  const conversionRate = orders.length > 0
    ? Math.round(((completedOrders.length) / orders.length) * 100)
    : 0;

  const stats = [
    { icon: ShoppingCart, label: 'Total Orders', value: orders.length.toString(), sub: `${pendingOrders} pending`, color: 'bg-blue-500/10 text-blue-600' },
    { icon: DollarSign, label: 'Total Revenue', value: `Rs. ${(totalRevenue / 1000).toFixed(1)}K`, sub: `Rs. ${todayRevenue.toLocaleString()} today`, color: 'bg-emerald-500/10 text-emerald-600' },
    { icon: Users, label: 'Pending Approvals', value: pendingSalesman.length.toString(), sub: 'Salesmen awaiting review', color: 'bg-amber-500/10 text-amber-600' },
    { icon: Activity, label: 'Conversion Rate', value: `${conversionRate}%`, sub: `${completedOrders.length} of ${orders.length} completed`, color: 'bg-violet-500/10 text-violet-600' },
  ];

  return (
    <div className="space-y-6">
      {/* Stat Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        {stats.map((stat, index) => {
          const Icon = stat.icon;
          return (
            <motion.div
              key={index}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.08 }}
              className="card group"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm font-medium">{stat.label}</p>
                  <p className="text-3xl font-extrabold text-gray-900 mt-1 tracking-tight">{stat.value}</p>
                  <p className="text-xs text-gray-400 mt-1">{stat.sub}</p>
                </div>
                <div className={`p-3 rounded-xl ${stat.color} transition-transform group-hover:scale-110`}>
                  <Icon className="w-6 h-6" />
                </div>
              </div>
            </motion.div>
          );
        })}
      </div>

      {/* Alerts */}
      {(lowStockProducts.length > 0 || outOfStockProducts.length > 0) && (
        <div className="grid sm:grid-cols-2 gap-4">
          {outOfStockProducts.length > 0 && (
            <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="p-4 bg-red-50 border border-red-200 rounded-2xl flex items-start gap-3">
              <div className="p-2 bg-red-100 rounded-xl flex-shrink-0">
                <Package className="w-5 h-5 text-red-500" />
              </div>
              <div>
                <p className="font-semibold text-red-800">Out of Stock</p>
                <p className="text-sm text-red-600 mt-0.5">{outOfStockProducts.length} product(s) have zero stock and cannot be ordered.</p>
                <div className="mt-2 space-y-1">
                  {outOfStockProducts.slice(0, 3).map(p => (
                    <p key={p.id} className="text-xs text-red-500 font-medium">• {p.name}</p>
                  ))}
                  {outOfStockProducts.length > 3 && <p className="text-xs text-red-400">+{outOfStockProducts.length - 3} more</p>}
                </div>
              </div>
            </motion.div>
          )}
          {lowStockProducts.length > 0 && (
            <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="p-4 bg-amber-50 border border-amber-200 rounded-2xl flex items-start gap-3">
              <div className="p-2 bg-amber-100 rounded-xl flex-shrink-0">
                <Package className="w-5 h-5 text-amber-500" />
              </div>
              <div>
                <p className="font-semibold text-amber-800">Low Stock Warning</p>
                <p className="text-sm text-amber-600 mt-0.5">{lowStockProducts.length} product(s) are at or below reorder level.</p>
                <div className="mt-2 space-y-1">
                  {lowStockProducts.slice(0, 3).map(p => (
                    <p key={p.id} className="text-xs text-amber-600 font-medium">• {p.name} ({p.stockQuantity} left)</p>
                  ))}
                  {lowStockProducts.length > 3 && <p className="text-xs text-amber-400">+{lowStockProducts.length - 3} more</p>}
                </div>
              </div>
            </motion.div>
          )}
        </div>
      )}

      {notifCount > 0 && (
        <motion.div initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }} className="p-4 bg-blue-50 border border-blue-200 rounded-2xl flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-blue-500 text-white text-sm font-bold flex items-center justify-center flex-shrink-0">{notifCount}</div>
          <p className="text-sm text-blue-700 font-medium">You have {notifCount} unread notification{notifCount > 1 ? 's' : ''} requiring attention.</p>
        </motion.div>
      )}

      <div className="grid lg:grid-cols-2 gap-6">
        {/* Order Status Breakdown */}
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }} className="card">
          <h3 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <TrendingUp className="w-5 h-5 text-primary-500" />
            Order Status Breakdown
          </h3>
          {orders.length === 0 ? (
            <p className="text-gray-400 text-sm text-center py-4">No orders yet.</p>
          ) : (
            <div className="space-y-3">
              {statusBreakdown.map(({ status, count, pct }) => {
                const color = status === 'Pending' ? 'bg-amber-400' :
                  status === 'Approved' ? 'bg-green-400' :
                  status === 'Dispatched' ? 'bg-blue-400' :
                  ['Delivered', 'Received'].includes(status) ? 'bg-emerald-500' :
                  'bg-red-400';
                return (
                  <div key={status}>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="font-medium text-gray-700">{status}</span>
                      <span className="text-gray-500">{count} orders ({pct}%)</span>
                    </div>
                    <div className="w-full bg-gray-100 rounded-full h-2">
                      <motion.div
                        initial={{ width: 0 }}
                        animate={{ width: `${pct}%` }}
                        transition={{ duration: 0.8, delay: 0.4 }}
                        className={`h-2 rounded-full ${color}`}
                      />
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </motion.div>

        {/* Top Products */}
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.35 }} className="card">
          <h3 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
            <CheckCircle className="w-5 h-5 text-emerald-500" />
            Best-Selling Products
          </h3>
          {topProducts.length === 0 ? (
            <p className="text-gray-400 text-sm text-center py-4">No sales data yet.</p>
          ) : (
            <div className="space-y-3">
              {topProducts.map((p, i) => (
                <div key={i} className="flex items-center justify-between p-3 bg-gray-50 rounded-xl">
                  <div className="flex items-center gap-3">
                    <div className={`w-7 h-7 rounded-lg flex items-center justify-center text-xs font-bold text-white ${i === 0 ? 'bg-amber-400' : i === 1 ? 'bg-gray-400' : i === 2 ? 'bg-orange-400' : 'bg-gray-200 text-gray-600'}`}>
                      {i + 1}
                    </div>
                    <div>
                      <p className="text-sm font-semibold text-gray-800 line-clamp-1">{p.name}</p>
                      <p className="text-xs text-gray-500">{p.count} units sold</p>
                    </div>
                  </div>
                  <p className="text-sm font-bold text-emerald-600">Rs. {p.revenue.toLocaleString()}</p>
                </div>
              ))}
            </div>
          )}
        </motion.div>
      </div>

      {/* Charts Section */}
      <div className="space-y-6">
        {/* Revenue Trend Chart */}
        {revenueChartData.length > 0 && (
          <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 }} className="card">
            <h3 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
              <TrendingUp className="w-5 h-5 text-blue-500" />
              Revenue Trend
            </h3>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={revenueChartData}>
                <defs>
                  <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.8}/>
                    <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" vertical={false} />
                <XAxis dataKey="date" stroke="#6b7280" axisLine={false} tickLine={false} />
                <YAxis stroke="#6b7280" axisLine={false} tickLine={false} />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '12px', color: '#fff', boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1)' }}
                  itemStyle={{ color: '#fff' }}
                  formatter={(value) => `Rs. ${Number(value).toLocaleString()}`}
                />
                <Area type="monotone" dataKey="revenue" stroke="#3b82f6" strokeWidth={3} fillOpacity={1} fill="url(#colorRevenue)" activeDot={{ r: 6, strokeWidth: 0, fill: '#3b82f6' }} />
              </AreaChart>
            </ResponsiveContainer>
          </motion.div>
        )}

        <div className="grid lg:grid-cols-2 gap-6">
          {/* Orders by City/Area — main featured chart */}
          {salesByCity.length > 0 && (
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.45 }}
              className="card lg:col-span-2"
            >
              {/* Header row */}
              <div className="flex flex-col sm:flex-row sm:items-center justify-between mb-6 gap-3">
                <div>
                  <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
                    <MapPin className="w-5 h-5 text-rose-500" />
                    Orders by City / Area
                  </h3>
                  <p className="text-xs text-gray-400 mt-0.5">Which area is driving the most orders?</p>
                </div>
                {/* Toggle */}
                <div className="flex items-center gap-1 bg-gray-100 rounded-xl p-1 w-fit">
                  <button
                    onClick={() => setCityViewMode('count')}
                    className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-all ${
                      cityViewMode === 'count'
                        ? 'bg-white text-rose-600 shadow-sm'
                        : 'text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    By Orders
                  </button>
                  <button
                    onClick={() => setCityViewMode('revenue')}
                    className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-all ${
                      cityViewMode === 'revenue'
                        ? 'bg-white text-emerald-600 shadow-sm'
                        : 'text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    By Revenue
                  </button>
                </div>
              </div>

              <div className="grid lg:grid-cols-5 gap-6 items-start">
                {/* Bar Chart */}
                <div className="lg:col-span-3">
                  <ResponsiveContainer width="100%" height={300}>
                    <BarChart
                      data={[...salesByCity]
                        .sort((a, b) =>
                          cityViewMode === 'count' ? b.count - a.count : b.revenue - a.revenue
                        )
                        .slice(0, 10)
                      }
                      margin={{ top: 4, right: 8, left: 0, bottom: 40 }}
                    >
                      <defs>
                        <linearGradient id="colorCityOrders" x1="0" y1="0" x2="0" y2="1">
                          <stop offset="5%" stopColor="#f43f5e" stopOpacity={0.95}/>
                          <stop offset="95%" stopColor="#fb7185" stopOpacity={0.6}/>
                        </linearGradient>
                        <linearGradient id="colorCityRevenue" x1="0" y1="0" x2="0" y2="1">
                          <stop offset="5%" stopColor="#10b981" stopOpacity={0.95}/>
                          <stop offset="95%" stopColor="#34d399" stopOpacity={0.6}/>
                        </linearGradient>
                      </defs>
                      <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" vertical={false} />
                      <XAxis
                        dataKey="city"
                        stroke="#9ca3af"
                        axisLine={false}
                        tickLine={false}
                        angle={-35}
                        textAnchor="end"
                        tick={{ fontSize: 11 }}
                        interval={0}
                      />
                      <YAxis
                        stroke="#9ca3af"
                        axisLine={false}
                        tickLine={false}
                        tick={{ fontSize: 11 }}
                        tickFormatter={cityViewMode === 'revenue' ? (v) => `${(v/1000).toFixed(0)}K` : undefined}
                      />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: '#111827',
                          border: '1px solid #1f2937',
                          borderRadius: '12px',
                          color: '#fff',
                          boxShadow: '0 20px 25px -5px rgba(0,0,0,0.2)',
                        }}
                        itemStyle={{ color: '#e5e7eb' }}
                        cursor={{ fill: 'rgba(244,63,94,0.07)' }}
                        formatter={(value: any) =>
                          cityViewMode === 'count'
                            ? [`${value} orders`, 'Orders']
                            : [`Rs. ${Number(value).toLocaleString()}`, 'Revenue']
                        }
                      />
                      <Bar
                        dataKey={cityViewMode === 'count' ? 'count' : 'revenue'}
                        fill={cityViewMode === 'count' ? 'url(#colorCityOrders)' : 'url(#colorCityRevenue)'}
                        radius={[8, 8, 0, 0]}
                        maxBarSize={52}
                      >
                        {[...salesByCity]
                          .sort((a, b) =>
                            cityViewMode === 'count' ? b.count - a.count : b.revenue - a.revenue
                          )
                          .slice(0, 10)
                          .map((_, index) => (
                            <Cell
                              key={`cell-${index}`}
                              fill={
                                index === 0
                                  ? (cityViewMode === 'count' ? '#f43f5e' : '#059669')
                                  : index === 1
                                  ? (cityViewMode === 'count' ? '#fb7185' : '#10b981')
                                  : (cityViewMode === 'count' ? 'url(#colorCityOrders)' : 'url(#colorCityRevenue)')
                              }
                              opacity={1 - index * 0.05}
                            />
                          ))
                        }
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </div>

                {/* Leaderboard */}
                <div className="lg:col-span-2">
                  <p className="text-xs font-semibold text-gray-400 uppercase tracking-widest mb-3">
                    {cityViewMode === 'count' ? 'Top Areas by Orders' : 'Top Areas by Revenue'}
                  </p>
                  <div className="space-y-2">
                    {[...salesByCity]
                      .sort((a, b) =>
                        cityViewMode === 'count' ? b.count - a.count : b.revenue - a.revenue
                      )
                      .slice(0, 8)
                      .map((item, i) => {
                        const maxVal = cityViewMode === 'count'
                          ? salesByCity[0]?.count || 1
                          : Math.max(...salesByCity.map(c => c.revenue)) || 1;
                        const val = cityViewMode === 'count' ? item.count : item.revenue;
                        const pct = Math.round((val / maxVal) * 100);
                        const medal = i === 0 ? '🥇' : i === 1 ? '🥈' : i === 2 ? '🥉' : null;
                        return (
                          <div key={i} className="group">
                            <div className="flex items-center justify-between mb-0.5">
                              <div className="flex items-center gap-1.5 min-w-0">
                                {medal
                                  ? <span className="text-base leading-none">{medal}</span>
                                  : <span className="w-5 h-5 rounded-full bg-gray-100 text-gray-500 text-xs font-bold flex items-center justify-center flex-shrink-0">{i + 1}</span>
                                }
                                <span className="text-sm font-medium text-gray-700 truncate">{item.city}</span>
                              </div>
                              <span className="text-xs font-bold ml-2 whitespace-nowrap"
                                style={{ color: cityViewMode === 'count' ? '#f43f5e' : '#059669' }}
                              >
                                {cityViewMode === 'count'
                                  ? `${item.count} orders`
                                  : `Rs. ${(item.revenue / 1000).toFixed(1)}K`
                                }
                              </span>
                            </div>
                            <div className="w-full bg-gray-100 rounded-full h-1.5">
                              <motion.div
                                initial={{ width: 0 }}
                                animate={{ width: `${pct}%` }}
                                transition={{ duration: 0.6, delay: i * 0.05 }}
                                className="h-1.5 rounded-full"
                                style={{
                                  background: cityViewMode === 'count'
                                    ? 'linear-gradient(90deg,#f43f5e,#fb7185)'
                                    : 'linear-gradient(90deg,#059669,#34d399)'
                                }}
                              />
                            </div>
                          </div>
                        );
                      })
                    }
                  </div>

                  {/* Summary badges */}
                  <div className="mt-5 pt-4 border-t border-gray-100 grid grid-cols-2 gap-3">
                    <div className="bg-rose-50 rounded-xl p-3 text-center border border-rose-100">
                      <p className="text-xl font-extrabold text-rose-600">
                        {salesByCity.reduce((s, c) => s + c.count, 0)}
                      </p>
                      <p className="text-xs text-rose-500 font-medium mt-0.5">Total Orders</p>
                    </div>
                    <div className="bg-emerald-50 rounded-xl p-3 text-center border border-emerald-100">
                      <p className="text-xl font-extrabold text-emerald-600">
                        {salesByCity.length}
                      </p>
                      <p className="text-xs text-emerald-500 font-medium mt-0.5">Cities Active</p>
                    </div>
                  </div>
                </div>
              </div>
            </motion.div>
          )}

          {/* Product Revenue Distribution */}
          {productRevenue.length > 0 && (
            <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.5 }} className="card">
              <h3 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                <Package className="w-5 h-5 text-purple-500" />
                Product Revenue Distribution
              </h3>
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={productRevenue}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent }: any) => `${name?.substring(0, 10) || ''}: ${((percent || 0) * 100).toFixed(0)}%`}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="revenue"
                  >
                    {productRevenue.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip 
                    contentStyle={{ backgroundColor: '#1f2937', border: 'none', borderRadius: '8px', color: '#fff' }}
                    formatter={(value) => `Rs. ${Number(value).toLocaleString()}`}
                  />
                </PieChart>
              </ResponsiveContainer>
            </motion.div>
          )}
        </div>

        {/* Employee Sales Performance */}
        {employeeRevenue.length > 0 && (
          <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.55 }} className="card">
            <h3 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
              <Users className="w-5 h-5 text-indigo-500" />
              Salesman Performance
            </h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={employeeRevenue}>
                <defs>
                  <linearGradient id="colorEmployee" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#10b981" stopOpacity={0.9}/>
                    <stop offset="95%" stopColor="#10b981" stopOpacity={0.6}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" vertical={false} />
                <XAxis dataKey="name" stroke="#6b7280" angle={-45} textAnchor="end" height={80} axisLine={false} tickLine={false} />
                <YAxis stroke="#6b7280" axisLine={false} tickLine={false} />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '12px', color: '#fff', boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1)' }}
                  itemStyle={{ color: '#fff' }}
                  cursor={{ fill: 'rgba(16, 185, 129, 0.1)' }}
                  formatter={(value) => `Rs. ${Number(value).toLocaleString()}`}
                />
                <Bar dataKey="revenue" fill="url(#colorEmployee)" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </motion.div>
        )}
        
        {/* Agent Earnings Section */}
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.6 }} className="card">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between mb-6 gap-4">
            <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
              <DollarSign className="w-5 h-5 text-emerald-500" />
              Agent Earnings Analysis
            </h3>
            <div className="flex items-center gap-3">
              <select
                className="input py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm"
                value={selectedSalesmanId}
                onChange={(e) => setSelectedSalesmanId(e.target.value)}
              >
                {salesmenList.length === 0 && <option value="">No agents available</option>}
                {salesmenList.map(s => (
                  <option key={s.id} value={s.id}>{s.name}</option>
                ))}
              </select>
              <select
                className="input py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm"
                value={agentDays}
                onChange={(e) => setAgentDays(Number(e.target.value))}
              >
                <option value={7}>Last 7 Days</option>
                <option value={14}>Last 14 Days</option>
                <option value={30}>Last 30 Days</option>
                <option value={90}>Last 90 Days</option>
              </select>
            </div>
          </div>

          {isLoadingEarnings ? (
            <div className="h-64 flex items-center justify-center">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-500"></div>
            </div>
          ) : agentEarningsData ? (
            <>
              <div className="mb-6 bg-emerald-50 rounded-2xl p-4 flex items-center justify-between border border-emerald-100">
                <div>
                  <p className="text-sm font-medium text-emerald-800">Total Earnings ({agentDays} Days)</p>
                  <p className="text-3xl font-bold text-emerald-600 mt-1">Rs. {agentEarningsData.totalEarnings.toLocaleString()}</p>
                </div>
                <div className="w-12 h-12 bg-emerald-100 rounded-full flex items-center justify-center">
                  <DollarSign className="w-6 h-6 text-emerald-600" />
                </div>
              </div>
              <ResponsiveContainer width="100%" height={250}>
                <AreaChart data={agentEarningsData.dailyBreakdown}>
                  <defs>
                    <linearGradient id="colorAgent" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#10b981" stopOpacity={0.8}/>
                      <stop offset="95%" stopColor="#10b981" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" vertical={false} />
                  <XAxis dataKey="date" stroke="#6b7280" axisLine={false} tickLine={false} minTickGap={20} />
                  <YAxis stroke="#6b7280" axisLine={false} tickLine={false} />
                  <Tooltip 
                    contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '12px', color: '#fff', boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1)' }}
                    itemStyle={{ color: '#fff' }}
                    formatter={(value) => `Rs. ${Number(value).toLocaleString()}`}
                  />
                  <Area type="monotone" dataKey="earnings" stroke="#10b981" strokeWidth={3} fillOpacity={1} fill="url(#colorAgent)" activeDot={{ r: 6, strokeWidth: 0, fill: '#10b981' }} />
                </AreaChart>
              </ResponsiveContainer>
            </>
          ) : (
            <div className="h-64 flex items-center justify-center text-gray-400">
              Select an agent to view earnings
            </div>
          )}
        </motion.div>
      </div>
    </div>
  );
};

export default AdminOverview;
