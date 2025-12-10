'use client';

import React, { useState } from 'react';
import { useAuth } from '@/features/auth/context/AuthContext';
import Button from '@/components/common/Button';
import Icon from '@/components/common/Icon';
import Modal from '@/components/ui/Modal';
import LoginForm from './LoginForm';
import RegisterForm from './RegisterForm';
import UserProfileCard from './UserProfileCard';

export default function AuthButton() {
  const { isAuthenticated, user, logout, isLoading } = useAuth();
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [showProfileModal, setShowProfileModal] = useState(false);
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');

  if (isLoading) {
    return (
      <Button variant="secondary" size="md" disabled>
        <Icon name="loading" size={18} className="animate-spin" />
      </Button>
    );
  }

  if (isAuthenticated && user) {
    return (
      <>
        <div className="flex items-center gap-2">
          <Button
            variant="secondary"
            size="md"
            onClick={() => setShowProfileModal(true)}
            className="flex items-center gap-2"
          >
            <div className="w-6 h-6 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white text-xs font-bold">
              {user.username.charAt(0).toUpperCase()}
            </div>
            <span className="hidden sm:inline">{user.username}</span>
          </Button>
          <Button
            variant="danger"
            size="md"
            onClick={logout}
          >
            <Icon name="logout" size={18} />
          </Button>
        </div>

        <Modal
          isOpen={showProfileModal}
          onClose={() => setShowProfileModal(false)}
          title="Your Profile"
        >
          <UserProfileCard />
        </Modal>
      </>
    );
  }

  return (
    <>
      <Button
        variant="primary"
        size="md"
        onClick={() => {
          setAuthMode('login');
          setShowAuthModal(true);
        }}
      >
        <Icon name="login" size={18} className="inline mr-2" />
        Sign In
      </Button>

      <Modal
        isOpen={showAuthModal}
        onClose={() => setShowAuthModal(false)}
        title={authMode === 'login' ? 'Sign In' : 'Create Account'}
      >
        {authMode === 'login' ? (
          <LoginForm
            onSuccess={() => setShowAuthModal(false)}
            onSwitchToRegister={() => setAuthMode('register')}
          />
        ) : (
          <RegisterForm
            onSuccess={() => setShowAuthModal(false)}
            onSwitchToLogin={() => setAuthMode('login')}
          />
        )}
      </Modal>
    </>
  );
}
