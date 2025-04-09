const Dashboard = () => {
  const stats = [
    { title: 'Tổng doanh thu', value: '520.000.000 ₫', change: '+12%', status: 'increase' },
    { title: 'Đơn hàng mới', value: '45', change: '+6%', status: 'increase' },
    { title: 'Khách hàng mới', value: '23', change: '-3%', status: 'decrease' },
    { title: 'Sản phẩm đã bán', value: '158', change: '+24%', status: 'increase' },
  ];

  const recentOrders = [
    { id: 'ORD1001', customer: 'Nguyễn Văn A', amount: '2.500.000 ₫', status: 'Đã thanh toán', date: '12/04/2023' },
    { id: 'ORD1002', customer: 'Trần Thị B', amount: '1.200.000 ₫', status: 'Đang xử lý', date: '12/04/2023' },
    { id: 'ORD1003', customer: 'Lê Văn C', amount: '3.600.000 ₫', status: 'Đã giao hàng', date: '11/04/2023' },
    { id: 'ORD1004', customer: 'Phạm Thị D', amount: '850.000 ₫', status: 'Đã thanh toán', date: '11/04/2023' },
    { id: 'ORD1005', customer: 'Hoàng Văn E', amount: '1.750.000 ₫', status: 'Đã hủy', date: '10/04/2023' },
  ];

  return (
    <div className="py-6">
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Tổng quan</h1>
      
      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat, index) => (
          <div key={index} className="card px-4 py-5">
            <dt className="text-sm font-medium text-gray-500 truncate">{stat.title}</dt>
            <dd className="mt-1 flex justify-between items-baseline">
              <div className="text-2xl font-semibold text-gray-900">{stat.value}</div>
              <div className={`flex items-baseline text-sm font-semibold ${
                stat.status === 'increase' ? 'text-green-600' : 'text-red-600'
              }`}>
                {stat.change}
                <svg className={`w-4 h-4 ml-1 ${
                  stat.status === 'increase' ? 'text-green-500' : 'text-red-500'
                }`} fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
                    d={stat.status === 'increase' 
                      ? "M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" 
                      : "M13 17h8m0 0v-8m0 8l-8-8-4 4-6-6"} 
                  />
                </svg>
              </div>
            </dd>
          </div>
        ))}
      </div>

      {/* Recent Orders */}
      <div className="mt-8">
        <h2 className="text-lg font-medium text-gray-900 mb-4">Đơn hàng gần đây</h2>
        <div className="table-container">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th scope="col" className="table-header">Mã đơn hàng</th>
                <th scope="col" className="table-header">Khách hàng</th>
                <th scope="col" className="table-header">Số tiền</th>
                <th scope="col" className="table-header">Trạng thái</th>
                <th scope="col" className="table-header">Ngày đặt</th>
                <th scope="col" className="table-header">Thao tác</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {recentOrders.map((order) => (
                <tr key={order.id}>
                  <td className="table-cell font-medium text-gray-900">{order.id}</td>
                  <td className="table-cell">{order.customer}</td>
                  <td className="table-cell">{order.amount}</td>
                  <td className="table-cell">
                    <span className={`badge ${
                      order.status === 'Đã thanh toán' ? 'badge-success' :
                      order.status === 'Đang xử lý' ? 'badge-info' :
                      order.status === 'Đã giao hàng' ? 'badge-success' :
                      'badge-danger'
                    }`}>
                      {order.status}
                    </span>
                  </td>
                  <td className="table-cell">{order.date}</td>
                  <td className="table-cell">
                    <button className="text-indigo-600 hover:text-indigo-900">Xem</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default Dashboard; 