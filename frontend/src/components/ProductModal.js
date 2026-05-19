import { useState, useEffect } from 'react';

const EMPTY = { name: '', description: '', price: '', stock: '', category: '' };

export default function ProductModal({ product, onSave, onClose, loading }) {
  const [form, setForm]   = useState(EMPTY);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (product) {
      setForm({
        name:        product.name        || '',
        description: product.description || '',
        price:       product.price       ?? '',
        stock:       product.stock       ?? '',
        category:    product.category    || '',
      });
    } else {
      setForm(EMPTY);
    }
    setErrors({});
  }, [product]);

  const set = (field) => (e) => {
    setForm((f) => ({ ...f, [field]: e.target.value }));
    setErrors((er) => ({ ...er, [field]: null }));
  };

  const validate = () => {
    const e = {};
    if (!form.name.trim())              e.name  = 'Name is required';
    if (form.price === '' || isNaN(form.price) || Number(form.price) < 0)
                                        e.price = 'Valid price required (≥ 0)';
    if (form.stock === '' || isNaN(form.stock) || Number(form.stock) < 0 || !Number.isInteger(Number(form.stock)))
                                        e.stock = 'Valid stock required (integer ≥ 0)';
    return e;
  };

  const handleSubmit = () => {
    const e = validate();
    if (Object.keys(e).length) { setErrors(e); return; }

    onSave({
      name:        form.name.trim(),
      description: form.description.trim() || null,
      price:       parseFloat(form.price),
      stock:       parseInt(form.stock, 10),
      category:    form.category.trim() || null,
    });
  };

  const isEdit = !!product;

  return (
    <div className="modal-overlay" onClick={(e) => e.target === e.currentTarget && onClose()}>
      <div className="modal">
        <div className="modal__header">
          <h2 className="modal__title">{isEdit ? 'Edit Product' : 'Add Product'}</h2>
          <button className="modal__close" onClick={onClose} aria-label="Close">×</button>
        </div>

        <div className="form-group">
          <label className="form-label">Name *</label>
          <input className="form-control" value={form.name} onChange={set('name')} placeholder="Product name" />
          {errors.name && <span className="form-error">{errors.name}</span>}
        </div>

        <div className="form-group">
          <label className="form-label">Description</label>
          <textarea
            className="form-control"
            value={form.description}
            onChange={set('description')}
            placeholder="Optional description"
            rows={3}
            style={{ resize: 'vertical' }}
          />
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
          <div className="form-group">
            <label className="form-label">Price (₹/$) *</label>
            <input className="form-control" type="number" min="0" step="0.01"
              value={form.price} onChange={set('price')} placeholder="0.00" />
            {errors.price && <span className="form-error">{errors.price}</span>}
          </div>
          <div className="form-group">
            <label className="form-label">Stock *</label>
            <input className="form-control" type="number" min="0" step="1"
              value={form.stock} onChange={set('stock')} placeholder="0" />
            {errors.stock && <span className="form-error">{errors.stock}</span>}
          </div>
        </div>

        <div className="form-group">
          <label className="form-label">Category</label>
          <input className="form-control" value={form.category} onChange={set('category')} placeholder="e.g. Electronics" />
        </div>

        <div className="modal__footer">
          <button className="btn btn-ghost" onClick={onClose} disabled={loading}>Cancel</button>
          <button className="btn btn-primary" onClick={handleSubmit} disabled={loading}>
            {loading ? <span className="spinner" /> : (isEdit ? 'Save Changes' : 'Add Product')}
          </button>
        </div>
      </div>
    </div>
  );
}
