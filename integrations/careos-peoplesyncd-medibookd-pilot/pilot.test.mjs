import test from 'node:test';
import assert from 'node:assert/strict';
import { simulateFlow, verifyAuditChain } from './pilot.mjs';

const visit = (opts={}) => ({agencyId:'agency-fiction-a',id:'visit-fake-01',clientId:'client-fake-01',caregiverId:'staff-fake-01',startAt:'2026-11-09T14:00:00Z',endAt:'2026-11-09T22:00:00Z',state:'completed',approved:true,...opts});
const scenario = (overrides={}) => ({ agencyId:'agency-fiction-a', employees:[{agencyId:'agency-fiction-a',id:'staff-fake-01',wageCents:2200}], clients:[{agencyId:'agency-fiction-a',id:'client-fake-01'}], visits:[visit()], ...overrides });

test('approved fictional visit makes an 8-hour timesheet, unpaid payroll preview, draft invoice, balanced ledger and traceable audits',()=>{
  const r=simulateFlow(scenario());
  assert.equal(r.mode,'FICTIONAL_SIMULATION_ONLY');
  assert.equal(r.timesheets[0].minutes,480);
  assert.equal(r.payrollPreview[0].grossWagesCents,17600);
  assert.equal(r.payrollPreview[0].paymentSent,false);
  assert.equal(r.payrollPreview[0].taxWithholdingCalculated,false);
  assert.equal(r.invoices[0].amountCents,28000);
  assert.equal(r.invoices[0].transmitted,false);
  assert.equal(r.ledger.length,2);
  assert.equal(r.ledger[0].lines[0].cents,r.ledger[0].lines[1].cents);
  assert.equal(verifyAuditChain(r.audit),true);
});

test('replay with the same source ID is idempotent',()=>{
  const r=simulateFlow(scenario({visits:[visit(),visit()]}));
  assert.equal(r.payrollPreview.length,1);
  assert.equal(r.invoices.length,1);
  assert.equal(r.ledger.length,2);
});

test('conflicting reuse of visit ID fails closed',()=>{
  assert.throws(()=>simulateFlow(scenario({visits:[visit(),visit({endAt:'2026-11-09T23:00:00Z'})]})),/Conflicting duplicate/);
});

test('cross-agency visit rejected',()=>{
  assert.throws(()=>simulateFlow(scenario({visits:[visit({agencyId:'agency-fiction-b'})]})),/Cross-agency/);
});

test('cross-agency worker rejected',()=>{
  assert.throws(()=>simulateFlow(scenario({employees:[{agencyId:'agency-fiction-b',id:'staff-fake-01',wageCents:2200}]})),/Cross-agency/);
});

test('pending or unapproved timesheets never become pay or invoices',()=>{
  assert.throws(()=>simulateFlow(scenario({visits:[visit({approved:false})]})),/approved completed/);
  assert.throws(()=>simulateFlow(scenario({visits:[visit({state:'scheduled'})]})),/approved completed/);
});

test('overlapping visits cannot be counted twice',()=>{
  assert.throws(()=>simulateFlow(scenario({visits:[visit(),visit({id:'visit-fake-02',startAt:'2026-11-09T20:00:00Z',endAt:'2026-11-09T23:00:00Z'})]})),/Overlapping/);
});

test('over-40 weekly hours block the preview pending reviewed overtime rules',()=>{
  const visits=[0,1,2,3].map(i=>visit({id:`v${i}`,startAt:`2026-11-${String(9+i).padStart(2,'0')}T08:00:00Z`,endAt:`2026-11-${String(9+i).padStart(2,'0')}T20:00:00Z`}));
  assert.throws(()=>simulateFlow(scenario({visits})),/Overtime hours/);
});

test('audit modification fails hash verification',()=>{
  const r=simulateFlow(scenario());
  r.audit[0].detail.minutes=5;
  assert.equal(verifyAuditChain(r.audit),false);
});

test('future or malformed timestamps not accepted',()=>{
  assert.throws(()=>simulateFlow(scenario({visits:[visit({endAt:'invalid'})]})),/Invalid visit timestamps/);
});
