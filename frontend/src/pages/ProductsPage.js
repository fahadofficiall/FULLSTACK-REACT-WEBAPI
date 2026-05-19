import { useState, useEffect, useCallback } from 'react';
import { productsApi } from '../api/api';
import ProductModal  from '../components/ProductModal';
import ConfirmDialog from '../components/ConfirmDialog';

function categoryBadge(cat) {
  const map = {
    electronics: 'badge-blue',
    office:      'badge-green',
    accessories: 'badge-yellow',
  };
  const cls = map[(cat || '').toLowerCase()] || 'badge-gray';
  return <span className={`badge ${cls}`}>{cat || '—'}</span>;
}

function formatPrice(price) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price);
}

export default function ProductsPage() {
  const [products, setProducts]     = useState([]);
  const [loading, setLoading]       = useState(true);
  const [error, setError]           = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // modal state
  const [showModal, setShowModal]   = useState(false);
  const [editProduct, setEditProduct] = useState(null);
  const [saving, setSaving]         = useState(false);

  // confirm delete state
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting]         = useState(false);

  // search
  const [search, setSearch] = useState('');

  // ── Load products ───────────────────────────────────────────────────────────
  const loadProducts = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const res = await productsApi.getAll();
      setProducts(res.data);
    } catch (err) {
      setError('Failed to load products.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadProducts(); }, [loadProducts]);

  // ── Flash success ───────────────────────────────────────────────────────────
  const flash = (msg) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(''), 3000);
  };

  // ── Open add modal ──────────────────────────────────────────────────────────
  const openAdd = () => { setEditProduct(null); setShowModal(true); };

  // ── Open edit modal ─────────────────────────────────────────────────────────
  const openEdit = (product) => { setEditProduct(product); setShowModal(true); };

  // ── Save (create or update) ─────────────────────────────────────────────────
  const handleSave = async (data) => {
    setSaving(true);
    try {
      if (editProduct) {
        await productsApi.update(editProduct.id, data);
        flash('Product updated successfully.');
      } else {
        await productsApi.create(data);
        flash('Product added successfully.');
      }
      setShowModal(false);
      await loadProducts();
    } catch (err) {
      const msg = err.response?.data?.message || 'Save failed.';
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  // ── Delete ──────────────────────────────────────────────────────────────────
  const confirmDelete = (product) => setDeleteTarget(product);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await productsApi.delete(deleteTarget.id);
      flash('Product deleted.');
      setDeleteTarget(null);
      await loadProducts();
    } catch (err) {
      setError('Delete failed.');
    } finally {
      setDeleting(false);
    }
  };

  // ── Filtered list ───────────────────────────────────────────────────────────
  const filtered = products.filter((p) => {
    const q = search.toLowerCase();
    return (
      p.name.toLowerCase().includes(q) ||
      (p.category || '').toLowerCase().includes(q) ||
      (p.description || '').toLowerCase().includes(q)
    );
  });

  return (
    <div className="page">
      {/* Header */}
      <div className="page-header">
        <div>
          <h1>Products</h1>
          <p>{products.length} total product{products.length !== 1 ? 's' : ''}</p>
        </div>
        <button className="btn btn-primary" onClick={openAdd}>
          + Add Product
        </button>
      </div>

      {/* Alerts */}
      {error      && <div className="alert alert-error"   onClick={() => setError('')}>{error} <span style={{ float:'right', cursor:'pointer' }}>×</span></div>}
      {successMsg && <div className="alert alert-success">{successMsg}</div>}

      {/* Search */}
      <div className="card mb-4" style={{ padding: '12px 16px' }}>
        <input
          className="form-control"
          style={{ maxWidth: 320 }}
          placeholder="Search by name, category or description…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {/* Table */}
      <div className="card">
        {loading ? (
          <div style={{ padding: 48, textAlign: 'center', color: 'var(--clr-muted)' }}>
            <div className="spinner" style={{ borderColor: 'rgba(0,0,0,.15)', borderTopColor: 'var(--clr-primary)', width: 28, height: 28, borderWidth: 3 }} />
            <p style={{ marginTop: 12 }}>Loading products…</p>
          </div>
        ) : filtered.length === 0 ? (
          <div style={{ padding: 48, textAlign: 'center', color: 'var(--clr-muted)' }}>
            {search ? `No products match "${search}".` : 'No products yet. Add one!'}
          </div>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Name</th>
                  <th>Category</th>
                  <th>Price</th>
                  <th>Stock</th>
                  <th>Updated</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((p) => (
                  <tr key={p.id}>
                    <td className="text-muted text-sm">{p.id}</td>
                    <td>
                      <div className="font-semibold">{p.name}</div>
                      {p.description && (
                        <div className="text-muted text-sm" style={{ maxWidth: 260, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                          {p.description}
                        </div>
                      )}
                    </td>
                    <td>{categoryBadge(p.category)}</td>
                    <td className="font-semibold">{formatPrice(p.price)}</td>
                    <td>
                      <span style={{ color: p.stock === 0 ? 'var(--clr-danger)' : p.stock < 20 ? 'var(--clr-warning)' : 'inherit' }}>
                        {p.stock}
                      </span>
                    </td>
                    <td className="text-muted text-sm">
                      {new Date(p.updatedAt).toLocaleDateString()}
                    </td>
                    <td>
                      <div className="flex gap-2">
                        <button className="btn btn-ghost" onClick={() => openEdit(p)}>
                          ✏️ Edit
                        </button>
                        <button className="btn btn-danger" onClick={() => confirmDelete(p)}>
                          🗑 Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Add / Edit Modal */}
      {showModal && (
        <ProductModal
          product={editProduct}
          onSave={handleSave}
          onClose={() => setShowModal(false)}
          loading={saving}
        />
      )}

      {/* Delete Confirmation */}
      {deleteTarget && (
        <ConfirmDialog
          message="Delete this product?"
          subMessage={`"${deleteTarget.name}" will be removed.`}
          onConfirm={handleDelete}
          onCancel={() => setDeleteTarget(null)}
          loading={deleting}
        />
      )}
    </div>
  );
}
