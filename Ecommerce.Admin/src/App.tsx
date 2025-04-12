import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useState, useEffect, useCallback } from 'react'
import MainLayout from './components/layouts/MainLayout'
import AuthLayout from './components/layouts/AuthLayout'
import Dashboard from './pages/Dashboard'
import Login from './pages/Login'
import { authStoreService } from './services/auth-store.service'
import CategoryList from './pages/categories/CategoryList'
import CategoryForm from './pages/categories/CategoryForm'

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const authStatus = authStoreService.isAuthenticated();
    setIsAuthenticated(authStatus);
    setIsLoading(false);
  }, []);

  const updateAuthStatus = useCallback(() => {
    setIsAuthenticated(authStoreService.isAuthenticated());
  }, []);

  const handleLogout = useCallback(async () => {
    try {
      await authStoreService.logout();
      setIsAuthenticated(false);
    } catch (error) {
      console.error('Error during logout:', error);
      setIsAuthenticated(false);
    }
  }, []);

  if (isLoading) {
    return <div>Đang tải...</div>;
  }

  return (
    <BrowserRouter>
      <Routes>
        {/* Auth Routes */}
        <Route path="/" element={<AuthLayout />}>
          <Route index element={<Navigate to="/login" replace />} />
          <Route 
            path="login" 
            element={
              isAuthenticated 
                ? <Navigate to="/dashboard" replace /> 
                : <Login onLoginSuccess={updateAuthStatus} />
            } 
          />
        </Route>
        
        {/* Protected Admin Routes */}
        <Route 
          path="/" 
          element={
            isAuthenticated ? <MainLayout onLogout={handleLogout} /> : <Navigate to="/login" replace />
          }
        >
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="categories" element={<CategoryList />} />
          <Route path="categories/create" element={<CategoryForm />} />
          <Route path="categories/edit/:id" element={<CategoryForm isEdit={true} />} />
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
