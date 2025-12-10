'use client'
import React,{Suspense} from "react";
import GameVsBot from "@/features/gameVsBot/gameVsBot.page"

export default function GamePage() {
  return (
    <Suspense fallback={
          <div className="min-h-screen flex items-center justify-center">
            <div className="text-2xl f`ont-bold text-gray-900 dark:text-white">
              Loading game...
            </div>
          </div>
        }>
          
      <GameVsBot/>
    </Suspense>
  );
}
