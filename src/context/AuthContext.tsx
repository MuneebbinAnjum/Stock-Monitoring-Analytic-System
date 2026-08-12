import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { User } from '../types';
import { AuthApi } from '../api/auth.api';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string, role: string) => Promise<User>;
  signup: (email: string, password: string, fullName: string, role: string) => Promise<User>;
  logout: () => void;
  loading: boolean;
  error: string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Check if user is logged in on mount
  useEffect(() => {
    const checkAuth = async () => {
      try {
        const token = sessionStorage.getItem('token');
        if (token) {
          const response = await AuthApi.me();
          const userData: User = {
            id: response.userId,
            email: response.email,
            fullName: response.fullName,
            role: response.role as 'Admin' | 'Salesman' | 'Buyer',
            approvalStatus: response.approvalStatus as 'Pending' | 'Approved' | 'Rejected' | undefined,
            phone: response.phone || undefined,
          };
          setUser(userData);
          setIsAuthenticated(true);
        }
      } catch (err) {
        console.error('Auth check failed:', err);
      } finally {
        setLoading(false);
      }
    };

    checkAuth();
  }, []);

  const login = useCallback(async (email: string, password: string, role: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await AuthApi.login(email, password, role);
      // Ensure backend returned a user with the expected role
      if (response.role && response.role !== role) {
        // Prevent logging in under a different role than selected
        throw new Error('Invalid credentials for the selected role');
      }
      // Prevent salesman from logging in before admin approval
      if (role === 'Salesman' && response.approvalStatus && response.approvalStatus !== 'Approved') {
        // Do not store tokens or mark as authenticated
        const tmpUser: User = {
          id: response.userId,
          email: response.email,
          fullName: response.fullName,
          role: response.role as 'Admin' | 'Salesman' | 'Buyer',
          approvalStatus: response.approvalStatus as 'Pending' | 'Approved' | 'Rejected' | undefined,
          phone: response.phone || undefined,
        };
        // keep user in memory but not authenticated
        setUser(tmpUser);
        setIsAuthenticated(false);
        // do not store tokens
        throw new Error('Account is pending admin approval');
      }
      const userData: User = {
        id: response.userId,
        email: response.email,
        fullName: response.fullName,
        role: response.role as 'Admin' | 'Salesman' | 'Buyer',
        approvalStatus: response.approvalStatus as 'Pending' | 'Approved' | 'Rejected' | undefined,
        phone: response.phone || undefined,
      };
      // Only store tokens if backend returned them (e.g., salesman registrations pending approval won't return tokens)
      if (response.token && (!userData.approvalStatus || userData.approvalStatus === 'Approved')) {
        sessionStorage.setItem('token', response.token);
      }
      if (response.refreshToken && (!userData.approvalStatus || userData.approvalStatus === 'Approved')) {
        sessionStorage.setItem('refreshToken', response.refreshToken);
      }
      sessionStorage.setItem('user', JSON.stringify(userData));
      setUser(userData);
      setIsAuthenticated(true);
      return userData;
    } catch (err: any) {
      const errorMessage = err.response?.data?.errors?.[0] || err.response?.data?.message || err.message || 'Login failed';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const signup = useCallback(async (email: string, password: string, fullName: string, role: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await AuthApi.register({ email, password, fullName, role });
      const userData: User = {
        id: response.userId,
        email: response.email,
        fullName: response.fullName,
        role: response.role as 'Admin' | 'Salesman' | 'Buyer',
        approvalStatus: response.approvalStatus as 'Pending' | 'Approved' | 'Rejected' | undefined,
        phone: response.phone || undefined,
      };
      // For salesman, registration may create an account in Pending state.
      // Only store tokens and mark authenticated if account is approved and tokens are present.
      if (response.token && userData.approvalStatus === 'Approved') {
        sessionStorage.setItem('token', response.token);
      }
      if (response.refreshToken && userData.approvalStatus === 'Approved') {
        sessionStorage.setItem('refreshToken', response.refreshToken);
      }
      // Keep the user info so UI can show pending status, but do not mark as authenticated if not approved
      sessionStorage.setItem('user', JSON.stringify(userData));
      setUser(userData);
      setIsAuthenticated(userData.approvalStatus === 'Approved');
      return userData;
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Signup failed';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('refreshToken');
    sessionStorage.removeItem('user');
    // Also clear from localStorage just in case it was stuck there from an old session
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setUser(null);
    setIsAuthenticated(false);
    setError(null);
  }, []);

  const value: AuthContextType = {
    user,
    isAuthenticated,
    login,
    signup,
    logout,
    loading,
    error,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
