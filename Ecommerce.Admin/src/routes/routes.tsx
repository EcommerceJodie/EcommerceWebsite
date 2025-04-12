import React from 'react';
import { Navigate, RouteObject } from 'react-router-dom';
import Dashboard from '../pages/Dashboard';
import Login from '../pages/Login';
import MainLayout from '../components/layouts/MainLayout';
import CategoryList from '../pages/categories/CategoryList';
import CategoryForm from '../pages/categories/CategoryForm';

// Guard component to check if user is authenticated
const RequireAuth: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const token = localStorage.getItem('token');
  if (!token) {
    // Redirect to login page if not authenticated
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
};

const routes: RouteObject[] = [
  {
    path: '/',
    element: (
      <RequireAuth>
        <MainLayout />
      </RequireAuth>
    ),
    children: [
      {
        path: '/',
        element: <Dashboard />,
      },
      // Category routes
      {
        path: '/categories',
        element: <CategoryList />,
      },
      {
        path: '/categories/create',
        element: <CategoryForm />,
      },
      {
        path: '/categories/edit/:id',
        element: <CategoryForm isEdit={true} />,
      },
      // Add other routes here as needed
    ],
  },
  {
    path: '/login',
    element: <Login />,
  },
  {
    path: '*',
    element: <Navigate to="/" replace />,
  },
];

export default routes; 