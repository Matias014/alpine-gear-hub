import { Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { RequireAuth } from './components/RequireAuth'
import ConversationPage from './pages/ConversationPage'
import ConversationsPage from './pages/ConversationsPage'
import CreateListingPage from './pages/CreateListingPage'
import EditListingPage from './pages/EditListingPage'
import HomePage from './pages/HomePage'
import ListingDetailPage from './pages/ListingDetailPage'
import ListingsPage from './pages/ListingsPage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/listings" element={<ListingsPage />} />
        <Route path="/listings/:id" element={<ListingDetailPage />} />
        <Route
          path="/listings/new"
          element={
            <RequireAuth>
              <CreateListingPage />
            </RequireAuth>
          }
        />
        <Route
          path="/listings/:id/edit"
          element={
            <RequireAuth>
              <EditListingPage />
            </RequireAuth>
          }
        />
        <Route
          path="/messages"
          element={
            <RequireAuth>
              <ConversationsPage />
            </RequireAuth>
          }
        />
        <Route
          path="/messages/:id"
          element={
            <RequireAuth>
              <ConversationPage />
            </RequireAuth>
          }
        />
      </Route>
    </Routes>
  )
}
