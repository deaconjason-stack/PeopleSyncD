"use client";

import { FormEvent, useState } from "react";

export default function LoginPage() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");

  function submit(event: FormEvent) {
    event.preventDefault();
    setMessage("Identity-provider sign-in will be enabled when the PeopleSyncD authentication authority is configured.");
  }

  return (
    <main style={{minHeight:"100vh",display:"grid",placeItems:"center",padding:24,background:"#f5f7fa"}}>
      <form onSubmit={submit} style={{width:"100%",maxWidth:440,background:"white",border:"1px solid #e1e6ed",borderRadius:16,padding:36,boxShadow:"0 12px 40px rgba(20,30,45,.06)"}}>
        <p className="eyebrow">PEOPLESYNCD ENTERPRISE</p>
        <h1 style={{marginBottom:8}}>Sign in</h1>
        <p style={{color:"#687487",marginBottom:28}}>Use your organization identity to access the PeopleSyncD workspace.</p>
        <label style={{display:"grid",gap:8,fontSize:13,fontWeight:600}}>Work email<input value={email} onChange={e=>setEmail(e.target.value)} type="email" required placeholder="you@company.com" style={{padding:12,border:"1px solid #ccd3dd",borderRadius:8,fontSize:15}} /></label>
        <button type="submit" style={{width:"100%",marginTop:20,padding:13,border:0,borderRadius:8,background:"#111827",color:"white",fontWeight:700}}>Continue with organization identity</button>
        {message && <p role="status" style={{marginTop:18,fontSize:13,color:"#687487"}}>{message}</p>}
      </form>
    </main>
  );
}
