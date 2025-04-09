import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import MainLayout from './components/layouts/MainLayout'
import AuthLayout from './components/layouts/AuthLayout'
import Dashboard from './pages/Dashboard'
import Login from './pages/Login'

function App() {
  const isAuthenticated = !!localStorage.getItem('adminUser');

  return (
    <BrowserRouter>
      <Routes>
        {/* Auth Routes */}
        <Route path="/" element={<AuthLayout />}>
          <Route index element={<Navigate to="/login" replace />} />
          <Route path="login" element={<Login />} />
        </Route>
        
        {/* Protected Admin Routes */}
        <Route 
          path="/" 
          element={
            isAuthenticated ? <MainLayout /> : <Navigate to="/login" replace />
          }
        >
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="categories" element={<div>Danh mục</div>} />
          <Route path="products" element={<div>Sản phẩm</div>} />
          <Route path="customers" element={<div>Khách hàng</div>} />
          <Route path="orders" element={<div>Đơn hàng</div>} />
          <Route path="reviews" element={<div>Đánh giá</div>} />
          <Route path="settings" element={<div>Cài đặt</div>} />
        </Route>
        
        {/* 404 Route */}
        <Route path="*" element={<div>Không tìm thấy trang</div>} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
