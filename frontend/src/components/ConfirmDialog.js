export default function ConfirmDialog({ message, subMessage, onConfirm, onCancel, loading }) {
  return (
    <div className="modal-overlay" onClick={(e) => e.target === e.currentTarget && onCancel()}>
      <div className="modal" style={{ maxWidth: 380 }}>
        <div className="confirm-icon">⚠️</div>
        <p className="confirm-msg">{message}</p>
        {subMessage && <p className="confirm-sub">{subMessage}</p>}
        <div className="modal__footer" style={{ justifyContent: 'center' }}>
          <button className="btn btn-ghost" onClick={onCancel} disabled={loading}>Cancel</button>
          <button className="btn btn-danger" onClick={onConfirm} disabled={loading}>
            {loading ? <span className="spinner" /> : 'Delete'}
          </button>
        </div>
      </div>
    </div>
  );
}
