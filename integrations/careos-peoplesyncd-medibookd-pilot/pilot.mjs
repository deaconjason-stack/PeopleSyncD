import { createHash } from 'node:crypto';

/**
 * Synthetic, in-memory adapter-contract exercise only.
 * NOT connected to live PeopleSyncD, MediBookD, CareOS, a bank, or payroll processor.
 * Values are integer US cents and UTC timestamps. No PII, PHI, credentials, or live payments.
 */
export function simulateFlow({ agencyId, visits, employees, clients, billRateCents = 3500 }) {
  if (!agencyId || !Array.isArray(visits) || !Array.isArray(employees) || !Array.isArray(clients)) {
    throw new Error('agencyId, visits, employees, clients required');
  }
  if (!Number.isSafeInteger(billRateCents) || billRateCents <= 0) throw new Error('Invalid billing rate');
  const byEmployee = new Map(employees.map(e => [e.id, e]));
  const byClient = new Map(clients.map(c => [c.id, c]));
  const seen = new Map();
  const intervals = new Map();
  const weeklyMinutes = new Map();
  const ordered = [];
  const audit = [];
  const ledger = [];
  let lastHash = 'GENESIS';
  const auditLog = (action, entityId, detail) => {
    const event = { sequence: audit.length + 1, agencyId, action, entityId, detail, previousHash: lastHash };
    event.hash = createHash('sha256').update(JSON.stringify(event)).digest('hex');
    audit.push(event);
    lastHash = event.hash;
  };
  function validateTenant(record) {
    if (record.agencyId !== agencyId) throw new Error('Cross-agency data rejected');
  }
  function journal(source, lines) {
    const debits = lines.filter(l => l.side === 'debit').reduce((n,l) => n + l.cents, 0);
    const credits = lines.filter(l => l.side === 'credit').reduce((n,l) => n + l.cents, 0);
    if (debits !== credits || debits <= 0) throw new Error('Unbalanced journal');
    ledger.push({ agencyId, source, lines });
    auditLog('ledger.preview', source, { debits, credits });
  }
  // Reject unsafe inputs before generating any candidate records.
  for (const v of visits) {
    validateTenant(v);
    if (v.state !== 'completed' || v.approved !== true) throw new Error('Only approved completed visits are eligible');
    const employee = byEmployee.get(v.caregiverId);
    const client = byClient.get(v.clientId);
    if (!employee || !client) throw new Error('Missing caregiver or client');
    validateTenant(employee); validateTenant(client);
    if (seen.has(v.id)) {
      if (JSON.stringify(seen.get(v.id)) !== JSON.stringify(v)) throw new Error('Conflicting duplicate visit ID');
      continue; // Idempotent replay, not another payroll line or invoice.
    }
    const start = Date.parse(v.startAt), end = Date.parse(v.endAt);
    if (!Number.isFinite(start) || !Number.isFinite(end) || end <= start || (end-start) % 60000 !== 0) {
      throw new Error('Invalid visit timestamps');
    }
    const minutes = (end - start) / 60000;
    if (minutes <= 0 || minutes > 24 * 60) throw new Error('Visit duration outside pilot range');
    const spans = intervals.get(v.caregiverId) ?? [];
    if (spans.some(([a,b]) => start < b && end > a)) throw new Error('Overlapping employee timesheets');
    spans.push([start,end]); intervals.set(v.caregiverId, spans);
    // Weekly overtime is not approximated: fail closed until an independently reviewed policy engine exists.
    const monday = new Date(start);
    monday.setUTCHours(0,0,0,0);
    monday.setUTCDate(monday.getUTCDate() - (monday.getUTCDay()+6)%7);
    const weekKey = `${v.caregiverId}:${monday.toISOString().slice(0,10)}`;
    const total = (weeklyMinutes.get(weekKey) ?? 0) + minutes;
    if (total > 2400) throw new Error('Overtime hours require reviewed rule engine; preview blocked');
    weeklyMinutes.set(weekKey,total);
    if (!Number.isSafeInteger(employee.wageCents) || employee.wageCents <= 0) throw new Error('Invalid wage');
    seen.set(v.id,v);
    ordered.push({ v, minutes, wageCents: Math.floor((minutes*employee.wageCents+30)/60), invoiceCents: Math.floor((minutes*billRateCents+30)/60) });
  }
  const timesheets = [], payrollPreview = [], invoices = [];
  for (const {v,minutes,wageCents,invoiceCents} of ordered) {
    timesheets.push({ agencyId, sourceVisitId:v.id, caregiverId:v.caregiverId, minutes, approved:true });
    auditLog('timesheet.imported',v.id,{minutes});
    payrollPreview.push({ agencyId, sourceVisitId:v.id, caregiverId:v.caregiverId, grossWagesCents:wageCents, taxWithholdingCalculated:false, paymentSent:false });
    auditLog('payroll.preview',v.id,{grossWagesCents:wageCents});
    invoices.push({ agencyId, sourceVisitId:v.id, clientId:v.clientId, amountCents:invoiceCents, status:'DRAFT', transmitted:false });
    auditLog('invoice.draft',v.id,{amountCents:invoiceCents});
    journal(`invoice:${v.id}`,[{account:'Accounts Receivable',side:'debit',cents:invoiceCents},{account:'Service Revenue',side:'credit',cents:invoiceCents}]);
    journal(`wages:${v.id}`,[{account:'Wage Expense',side:'debit',cents:wageCents},{account:'Wages Payable',side:'credit',cents:wageCents}]);
  }
  return { mode:'FICTIONAL_SIMULATION_ONLY', agencyId, timesheets, payrollPreview, invoices, ledger, audit };
}

export function verifyAuditChain(events) {
  let previousHash = 'GENESIS';
  for (let i=0;i<events.length;i++) {
    const { hash, ...record } = events[i];
    if (record.sequence !== i+1 || record.previousHash !== previousHash) return false;
    if (createHash('sha256').update(JSON.stringify(record)).digest('hex') !== hash) return false;
    previousHash = hash;
  }
  return true;
}
