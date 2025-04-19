import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useState, useEffect, useCallback } from 'react'
import MainLayout from './components/layouts/MainLayout'
import AuthLayout from './components/layouts/AuthLayout'
import Dashboard from './pages/Dashboard'
import Login from './pages/Login'
import { authStoreService } from './services/auth-store.service'
import CategoryList from './pages/categories/CategoryList'
import CategoryForm from './pages/categories/CategoryForm'
import ProductList from './pages/products/ProductList'
import ProductForm from './pages/products/ProductForm'
import MenuConfigList from './pages/menuconfigs/MenuConfigList'
import MenuConfigForm from './pages/menuconfigs/MenuConfigForm'

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  // Xử lý xác thực khi ứng dụng khởi động
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        // Kiểm tra trạng thái xác thực ban đầu
        let authStatus = authStoreService.isAuthenticated();
        
        // Nếu có token nhưng đã hết hạn, thử refresh token
        if (!authStatus && authStoreService.getRefreshToken()) {
          // Thử refresh token
          const refreshSuccess = await authStoreService.refreshAccessToken();
          authStatus = refreshSuccess;
        }
        
        setIsAuthenticated(authStatus);
      } catch (error) {
        console.error('Error during authentication check:', error);
        setIsAuthenticated(false);
      } finally {
        setIsLoading(false);
      }
    };
    
    initializeAuth();
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
          
          {/* Categories Routes */}
          <Route path="categories" element={<CategoryList />} />
          <Route path="categories/create" element={<CategoryForm />} />
          <Route path="categories/edit/:id" element={<CategoryForm isEdit={true} />} />
          
          {/* Menu Config Routes */}
          <Route path="menu-configs" element={<MenuConfigList />} />
          <Route path="menu-configs/create" element={<MenuConfigForm />} />
          <Route path="menu-configs/edit/:id" element={<MenuConfigForm isEdit={true} />} />
          
          {/* Products Routes */}
          <Route path="products" element={<ProductList />} />
          <Route path="products/create" element={<ProductForm />} />
          <Route path="products/edit/:id" element={<ProductForm isEdit={true} />} />
          
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
