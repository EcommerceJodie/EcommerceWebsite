import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { authStoreService } from '../../services/auth-store.service';

interface HeaderProps {
  onLogout?: () => void;
}

const Header = ({ onLogout }: HeaderProps) => {
  const navigate = useNavigate();
  const [isProfileOpen, setIsProfileOpen] = useState(false);
  const [user, setUser] = useState({
    name: 'Admin',
    email: '',
    avatar: 'https://randomuser.me/api/portraits/men/1.jpg'
  });

  // Lấy thông tin người dùng khi component được mount
  useEffect(() => {
    const currentUser = authStoreService.getCurrentUser();
    if (currentUser) {
      setUser({
        name: currentUser.firstName && currentUser.lastName
          ? `${currentUser.firstName} ${currentUser.lastName}`
          : 'Admin',
        email: currentUser.email,
        avatar: 'https://randomuser.me/api/portraits/men/1.jpg' // Sử dụng avatar mặc định
      });
    }
  }, []);

  const toggleProfile = () => {
    setIsProfileOpen(!isProfileOpen);
  };

  const handleLogout = async () => {
    // Đóng dropdown profile
    setIsProfileOpen(false);
    
    // Gọi hàm onLogout được truyền từ App component (nếu có)
    if (onLogout) {
      await onLogout();
    } else {
      // Fallback: Đăng xuất người dùng
      try {
        await authStoreService.logout();
        // Chuyển hướng về trang đăng nhập
        navigate('/login', { replace: true });
      } catch (error) {
        console.error('Lỗi khi đăng xuất:', error);
        // Vẫn chuyển hướng về trang đăng nhập ngay cả khi có lỗi
        navigate('/login', { replace: true });
      }
    }
  };

  return (
    <header className="bg-white border-b border-gray-200 z-10">
      <div className="px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          <div className="flex">
            {/* Mobile menu button */}
            <button type="button" className="inline-flex items-center justify-center rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-indigo-500 lg:hidden">
              <span className="sr-only">Mở menu</span>
              <svg className="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
            
            {/* Search */}
            <div className="ml-4 flex md:ml-6">
              <div className="relative w-64">
                <input 
                  type="text" 
                  placeholder="Tìm kiếm..." 
                  className="block w-full rounded-md border-0 py-1.5 pl-10 pr-3 text-gray-900 ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-500 sm:text-sm sm:leading-6"
                />
                <div className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                  <svg className="h-5 w-5 text-gray-400" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z" clipRule="evenodd" />
                  </svg>
                </div>
              </div>
            </div>
          </div>

          {/* Right section */}
          <div className="flex items-center">
            {/* Notifications */}
            <button className="p-1 rounded-full text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
              <span className="sr-only">Xem thông báo</span>
              <svg className="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
              </svg>
            </button>

            {/* Profile dropdown */}
            <div className="ml-3 relative">
              <div>
                <button 
                  onClick={toggleProfile}
                  className="flex items-center max-w-xs rounded-full bg-white text-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500" 
                  id="user-menu-button" 
                  aria-expanded="false" 
                  aria-haspopup="true"
                >
                  <span className="sr-only">Mở menu người dùng</span>
                  <img className="h-8 w-8 rounded-full" src={user.avatar} alt="" />
                  <span className="ml-2 text-gray-700 hidden md:block">{user.name}</span>
                  <span className="ml-1 text-xs text-gray-500 hidden md:block">{user.email}</span>
                  <svg className="ml-1 h-5 w-5 text-gray-400 hidden md:block" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clipRule="evenodd" />
                  </svg>
                </button>
              </div>

              {isProfileOpen && (
                <div 
                  className="origin-top-right absolute right-0 mt-2 w-48 rounded-md shadow-lg py-1 bg-white ring-1 ring-black ring-opacity-5 focus:outline-none" 
                  role="menu" 
                  aria-orientation="vertical" 
                  aria-labelledby="user-menu-button"
                >
                  <Link to="/profile" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100" role="menuitem">
                    Hồ sơ của bạn
                  </Link>
                  <Link to="/settings" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100" role="menuitem">
                    Cài đặt
                  </Link>
                  <button 
                    onClick={handleLogout}
                    className="w-full text-left block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100" 
                    role="menuitem"
                  >
                    Đăng xuất
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header; 