import { useEffect, useState } from 'react'
import './App.css'

const API = import.meta.env.VITE_API_URL || 'https://localhost:7220/api'

async function request(path, options = {}) {
  const response = await fetch(`${API}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })
  const body = await response.json().catch(() => ({}))
  if (!response.ok) throw new Error(body.error || 'The request could not be completed.')
  return body
}

function App() {
  const [section, setSection] = useState('dashboard')
  const [customers, setCustomers] = useState({ items: [], totalCount: 0 })
  const [accounts, setAccounts] = useState({ items: [], totalCount: 0 })
  const [transactions, setTransactions] = useState({ items: [], totalCount: 0 })
  const [notice, setNotice] = useState(null)
  const [loading, setLoading] = useState(false)

  async function loadData() {
    setLoading(true)
    try {
      const [customerData, accountData, transactionData] = await Promise.all([
        request('/customers?pageSize=100'),
        request('/accounts?pageSize=100'),
        request('/transactions?pageSize=10'),
      ])
      setCustomers(customerData)
      setAccounts(accountData)
      setTransactions(transactionData)
    } catch (error) {
      setNotice({ type: 'error', text: error.message })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { loadData() }, [])

  async function submitTransaction(path, payload) {
    try {
      await request(path, { method: 'POST', body: JSON.stringify(payload) })
      setNotice({ type: 'success', text: 'Transaction completed successfully.' })
      await loadData()
      setSection('transactions')
    } catch (error) {
      setNotice({ type: 'error', text: error.message })
    }
  }

  const totalBalance = accounts.items.reduce((sum, account) => sum + account.balance, 0)
  const deposits = transactions.items.filter((item) => item.type === 'DEPOSIT').reduce((sum, item) => sum + item.amount, 0)
  const withdrawals = transactions.items.filter((item) => item.type === 'WITHDRAWAL').reduce((sum, item) => sum + item.amount, 0)

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand"><span className="brand-mark">B</span><span>BankCore</span></div>
        <p className="eyebrow">Portfolio banking MVP</p>
        <nav>
          {[
            ['dashboard', 'Overview', '⌂'],
            ['customers', 'Customers', '♙'],
            ['accounts', 'Accounts', '▣'],
            ['transactions', 'Transactions', '↔'],
          ].map(([key, label, icon]) => (
            <button className={section === key ? 'nav-item active' : 'nav-item'} key={key} onClick={() => setSection(key)}>
              <span>{icon}</span>{label}
            </button>
          ))}
        </nav>
        <div className="sidebar-foot">Demo environment<br /><strong>SQL Server API</strong></div>
      </aside>
      <main className="main-content">
        <header className="topbar"><div><p className="eyebrow">Good morning</p><h1>{section === 'dashboard' ? 'Financial overview' : section[0].toUpperCase() + section.slice(1)}</h1></div><button className="refresh" onClick={loadData}>↻ Refresh</button></header>
        {notice && <div className={`notice ${notice.type}`} role="alert">{notice.text}<button onClick={() => setNotice(null)}>×</button></div>}
        {loading && <div className="loading">Loading live data…</div>}
        {section === 'dashboard' && <Dashboard customers={customers} accounts={accounts} transactions={transactions} totalBalance={totalBalance} deposits={deposits} withdrawals={withdrawals} />}
        {section === 'customers' && <Customers customers={customers} onCreated={loadData} onError={(text) => setNotice({ type: 'error', text })} />}
        {section === 'accounts' && <Accounts accounts={accounts} customers={customers} onCreated={loadData} onError={(text) => setNotice({ type: 'error', text })} />}
        {section === 'transactions' && <Transactions transactions={transactions} onSubmit={submitTransaction} />}
      </main>
    </div>
  )
}

function Dashboard({ customers, accounts, transactions, totalBalance, deposits, withdrawals }) {
  return <><section className="hero-panel"><div><p className="eyebrow">Total managed balance</p><div className="balance">LKR {totalBalance.toLocaleString('en-LK', { minimumFractionDigits: 2 })}</div><p className="muted">Across {accounts.totalCount} active and inactive accounts</p></div><div className="hero-icon">◈</div></section>
    <div className="metrics"><Metric label="Total customers" value={customers.totalCount} icon="♙" /><Metric label="Total accounts" value={accounts.totalCount} icon="▣" /><Metric label="Deposits shown" value={`LKR ${deposits.toLocaleString()}`} icon="↗" /><Metric label="Withdrawals shown" value={`LKR ${withdrawals.toLocaleString()}`} icon="↘" /></div>
    <section className="panel"><div className="panel-title"><div><p className="eyebrow">Activity</p><h2>Recent transactions</h2></div><span className="pill">{transactions.totalCount} total</span></div><TransactionTable transactions={transactions.items} /></section>
  </>
}

function Metric({ label, value, icon }) { return <div className="metric"><span className="metric-icon">{icon}</span><p className="muted">{label}</p><strong>{value}</strong></div> }

function Customers({ customers, onCreated, onError }) {
  const [form, setForm] = useState({ fullName: '', nic: '', email: '', phoneNumber: '', address: '' })
  async function create(event) {
    event.preventDefault()
    try { await request('/customers', { method: 'POST', body: JSON.stringify(form) }); setForm({ fullName: '', nic: '', email: '', phoneNumber: '', address: '' }); await onCreated() }
    catch (error) { onError(error.message) }
  }
  return <div className="content-grid"><section className="panel"><div className="panel-title"><h2>Register customer</h2></div><form onSubmit={create}><Input label="Full name" value={form.fullName} onChange={(v) => setForm({ ...form, fullName: v })} required /><Input label="NIC number" value={form.nic} onChange={(v) => setForm({ ...form, nic: v })} required /><Input label="Email" type="email" value={form.email} onChange={(v) => setForm({ ...form, email: v })} required /><Input label="Phone number" value={form.phoneNumber} onChange={(v) => setForm({ ...form, phoneNumber: v })} required /><Input label="Address" value={form.address} onChange={(v) => setForm({ ...form, address: v })} required /><button className="primary">Create customer</button></form></section><section className="panel"><div className="panel-title"><h2>Customer directory</h2><span className="pill">{customers.totalCount}</span></div><div className="table-wrap"><table><thead><tr><th>Name</th><th>NIC</th><th>Email</th><th>Status</th></tr></thead><tbody>{customers.items.map((c) => <tr key={c.id}><td><strong>{c.fullName}</strong></td><td>{c.nic}</td><td>{c.email}</td><td><span className={`status ${c.isActive ? 'on' : 'off'}`}>{c.isActive ? 'Active' : 'Inactive'}</span></td></tr>)}</tbody></table></div></section></div>
}

function Accounts({ accounts, customers, onCreated, onError }) {
  const [form, setForm] = useState({ accountNumber: '', customerId: customers.items[0]?.id || '', accountType: 'Savings', openingBalance: 0, currency: 'LKR' })
  async function create(event) { event.preventDefault(); try { await request('/accounts', { method: 'POST', body: JSON.stringify({ ...form, customerId: Number(form.customerId), openingBalance: Number(form.openingBalance) }) }); await onCreated() } catch (error) { onError(error.message) } }
  return <div className="content-grid"><section className="panel"><div className="panel-title"><h2>Open account</h2></div><form onSubmit={create}><Input label="Account number" value={form.accountNumber} onChange={(v) => setForm({ ...form, accountNumber: v })} required /><label>Customer<select value={form.customerId} onChange={(e) => setForm({ ...form, customerId: e.target.value })}>{customers.items.map((c) => <option key={c.id} value={c.id}>{c.fullName}</option>)}</select></label><label>Account type<select value={form.accountType} onChange={(e) => setForm({ ...form, accountType: e.target.value })}><option>Savings</option><option>Current</option></select></label><Input label="Opening balance" type="number" min="0" value={form.openingBalance} onChange={(v) => setForm({ ...form, openingBalance: v })} required /><button className="primary">Open account</button></form></section><section className="panel"><div className="panel-title"><h2>Account portfolio</h2><span className="pill">{accounts.totalCount}</span></div><div className="table-wrap"><table><thead><tr><th>Account</th><th>Customer</th><th>Type</th><th>Balance</th></tr></thead><tbody>{accounts.items.map((a) => <tr key={a.id}><td><strong>{a.accountNumber}</strong></td><td>{a.customerName}</td><td>{a.accountType}</td><td>LKR {a.balance.toLocaleString('en-LK', { minimumFractionDigits: 2 })}</td></tr>)}</tbody></table></div></section></div>
}

function Transactions({ transactions, onSubmit }) {
  const [form, setForm] = useState({ accountNumber: '', amount: '', description: '' })
  const [transfer, setTransfer] = useState({ fromAccountNumber: '', toAccountNumber: '', amount: '', description: '' })
  return <><section className="transaction-actions"><ActionForm title="Deposit funds" form={form} setForm={setForm} submit={() => onSubmit('/transactions/deposit', { ...form, amount: Number(form.amount) })} /><ActionForm title="Withdraw funds" form={form} setForm={setForm} submit={() => onSubmit('/transactions/withdraw', { ...form, amount: Number(form.amount) })} /><div className="panel action-card"><h2>Transfer funds</h2><Input label="From account" value={transfer.fromAccountNumber} onChange={(v) => setTransfer({ ...transfer, fromAccountNumber: v })} /><Input label="To account" value={transfer.toAccountNumber} onChange={(v) => setTransfer({ ...transfer, toAccountNumber: v })} /><Input label="Amount" type="number" min="0.01" value={transfer.amount} onChange={(v) => setTransfer({ ...transfer, amount: v })} /><button className="primary" onClick={() => onSubmit('/transactions/transfer', { ...transfer, amount: Number(transfer.amount) })}>Transfer</button></div></section><section className="panel"><div className="panel-title"><h2>Transaction history</h2></div><TransactionTable transactions={transactions.items} /></section></>
}

function ActionForm({ title, form, setForm, submit }) { return <div className="panel action-card"><h2>{title}</h2><Input label="Account number" value={form.accountNumber} onChange={(v) => setForm({ ...form, accountNumber: v })} /><Input label="Amount" type="number" min="0.01" value={form.amount} onChange={(v) => setForm({ ...form, amount: v })} /><button className="primary" onClick={submit}>Submit</button></div> }
function Input({ label, value, onChange, ...props }) { return <label>{label}<input value={value} onChange={(e) => onChange(e.target.value)} {...props} /></label> }
function TransactionTable({ transactions }) { return <div className="table-wrap"><table><thead><tr><th>Reference</th><th>Account</th><th>Type</th><th>Amount</th><th>New balance</th></tr></thead><tbody>{transactions.map((t) => <tr key={t.id}><td><strong>{t.referenceNumber}</strong></td><td>{t.accountNumber}</td><td><span className="type">{t.type.replace('_', ' ')}</span></td><td>LKR {t.amount.toLocaleString('en-LK', { minimumFractionDigits: 2 })}</td><td>LKR {t.newBalance.toLocaleString('en-LK', { minimumFractionDigits: 2 })}</td></tr>)}{transactions.length === 0 && <tr><td colSpan="5" className="empty">No transactions recorded yet.</td></tr>}</tbody></table></div> }

export default App
