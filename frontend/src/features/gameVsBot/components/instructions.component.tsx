export function Instructions() {
    return (
        <div className="mt-4 mx-auto max-w-7xl bg-white dark:bg-gray-800 rounded-lg shadow-lg p-4">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                How to Play
            </h3>
            <ul className="text-sm text-gray-700 dark:text-gray-300 space-y-1">
                <li>• Click on your pawn, then click on a valid cell to move</li>
                <li>• Click between cells to place walls (horizontal or vertical)</li>
                <li>• Walls must not completely block any player's path to their goal</li>
                <li>• Reach the opposite side to win!</li>
            </ul>
        </div>
    );
}