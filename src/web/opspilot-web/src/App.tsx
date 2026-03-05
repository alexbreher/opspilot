import ".\App.css";

export default function App() {
    return (
        <div style={{ padding: 24, fontFamily: "system-ui, sans-serif" }}>
            <h1>OpsPilot</h1>
            <p>Multi-tenant Incident = Runbook platform (portfolio build)</p>

            <section style={{ marginTop: 24, padding: 16, border: "1px solid #ddd", borderRadius: 12 }}>
                <h2>Auth (placeholder)</h2>
                <p>Status: Not implemented yet.</p>
                <button disabled>Sign in</button>
                <button disabled style={{marginLeft:8} }>Sign out</button>
            </section>

            <section style={{ marginTop: 24 }}>
                <h2>API</h2>
                <p>Planned: /api endpoints via AKS ingress</p>
            </section>
        </div>
    );
}