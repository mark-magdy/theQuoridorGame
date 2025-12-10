import React from 'react';
import type { Metadata } from 'next';
import '@/styles/globals.css';
import { AuthProvider } from '@/features/auth/context/AuthContext';

export const metadata: Metadata = {
  title: 'Quoridor Game - Strategic Board Game',
  description: 'Play the classic Quoridor board game. Strategic gameplay with 2-4 players.',
  keywords: ['quoridor', 'board game', 'strategy game', 'multiplayer'],
  authors: [{ name: 'Quoridor Game' }],
  viewport: 'width=device-width, initial-scale=1',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800">
        <AuthProvider>
          {children}
        </AuthProvider>
      </body>
    </html>
  );
}

