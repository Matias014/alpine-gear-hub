import { Link, Outlet } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

export function Layout() {
  const { user, isAuthenticated, logout } = useAuth()

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="border-b border-gray-200 bg-white">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
          <Link to="/" className="text-lg font-bold text-gray-900">
            AlpineGearHub
          </Link>

          <nav className="flex items-center gap-4 text-sm">
            <Link to="/listings" className="text-gray-600 hover:text-gray-900">
              Browse
            </Link>

            {isAuthenticated && user ? (
              <>
                <Link to="/listings/new" className="text-gray-600 hover:text-gray-900">
                  Sell gear
                </Link>
                <Link to={`/listings?sellerId=${user.id}`} className="text-gray-600 hover:text-gray-900">
                  My listings
                </Link>
                <button type="button" onClick={logout} className="text-gray-600 hover:text-gray-900">
                  Log out
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="text-gray-600 hover:text-gray-900">
                  Log in
                </Link>
                <Link to="/register" className="font-medium text-emerald-700 hover:underline">
                  Register
                </Link>
              </>
            )}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}
