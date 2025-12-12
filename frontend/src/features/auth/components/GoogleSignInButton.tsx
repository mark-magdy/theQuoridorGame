'use client';

import React from 'react';
import { GoogleOAuthProvider, GoogleLogin, CredentialResponse } from '@react-oauth/google';
import { useAuth } from '@/features/auth/context/AuthContext';
import { ApiError } from '@/lib/apiClient';

interface GoogleSignInButtonProps {
  onSuccess?: () => void;
  onError?: (error: string) => void;
  text?: 'signin_with' | 'signup_with' | 'continue_with' | 'signin';
}

function GoogleSignInButtonInner({ onSuccess, onError, text = 'continue_with' }: GoogleSignInButtonProps) {
  const { googleAuth } = useAuth();

  const handleSuccess = async (credentialResponse: CredentialResponse) => {
    try {
      if (credentialResponse.credential) {
        await googleAuth(credentialResponse.credential);
        onSuccess?.();
      }
    } catch (err) {
      if (err instanceof ApiError) {
        onError?.(err.message);
      } else {
        onError?.('Failed to authenticate with Google');
      }
    }
  };

  const handleError = () => {
    onError?.('Google authentication failed');
  };

  return (
    <div className="flex justify-center">
      <GoogleLogin
        onSuccess={handleSuccess}
        onError={handleError}
        type='standard'
        text={text}
        size="large"
        width="100%"
        theme="filled_black"
        shape="pill"
        locale="en"
      />
    </div>
  );
}

export default function GoogleSignInButton(props: GoogleSignInButtonProps) {
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

  if (!clientId) {
    return (
      <div className="text-sm text-gray-500 text-center">
        Google Sign-In not configured
      </div>
    );
  }

  return (
    <GoogleOAuthProvider clientId={clientId} >
      <GoogleSignInButtonInner {...props} />
    </GoogleOAuthProvider>
  );
}
