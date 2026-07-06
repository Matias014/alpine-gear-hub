import { Link, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { useUnreadMessagesCount } from '../hooks/useChat'
import { useChatNotifications } from '../hooks/useChatNotifications'

export function Layout() {
  const { user, isAuthenticated, logout } = useAuth()
  const unreadCount = useUnreadMessagesCount()
  useChatNotifications()
  const navigate = useNavigate()

  // Protected pages already redirect on logout via RequireAuth/RequireModerator, but public
  // pages (home, browse, listing detail) don't - without this, logging out while on e.g.
  // "My listings" just silently updates the nav and leaves you on a now-meaningless filtered view.
  function handleLogout() {
    logout()
    navigate('/')
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="sticky top-0 z-10 border-b border-gray-200 bg-white/95 backdrop-blur">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
          <Link to="/" className="text-lg font-bold tracking-tight text-gray-900">
            Alpine<span className="text-emerald-600">Gear</span>Hub
          </Link>

          <nav className="flex items-center gap-5 text-sm">
            <Link to="/listings" className="text-gray-600 transition-colors hover:text-gray-900">
              Browse
            </Link>

            {isAuthenticated && user ? (
              <>
                <Link
                  to={`/listings?sellerId=${user.id}`}
                  className="text-gray-600 transition-colors hover:text-gray-900"
                >
                  My listings
                </Link>
                <Link to="/messages" className="relative text-gray-600 transition-colors hover:text-gray-900">
                  Messages
                  {unreadCount > 0 && (
                    <span className="absolute -right-3 -top-2 rounded-full bg-emerald-600 px-1.5 py-0.5 text-[10px] font-medium text-white">
                      {unreadCount}
                    </span>
                  )}
                </Link>
                {(user.role === 'Moderator' || user.role === 'Admin') && (
                  <Link to="/moderation" className="text-gray-600 transition-colors hover:text-gray-900">
                    Moderation
                  </Link>
                )}
                <button
                  type="button"
                  onClick={handleLogout}
                  className="text-gray-600 transition-colors hover:text-gray-900"
                >
                  Log out
                </button>
                <Link
                  to="/listings/new"
                  className="rounded-lg bg-emerald-600 px-3 py-1.5 font-semibold text-white shadow-sm transition-colors hover:bg-emerald-500"
                >
                  Sell gear
                </Link>
              </>
            ) : (
              <>
                <Link to="/login" className="text-gray-600 transition-colors hover:text-gray-900">
                  Log in
                </Link>
                <Link
                  to="/register"
                  className="rounded-lg bg-emerald-600 px-3 py-1.5 font-semibold text-white shadow-sm transition-colors hover:bg-emerald-500"
                >
                  Register
                </Link>
              </>
            )}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}
