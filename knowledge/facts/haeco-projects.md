# HAECO systems (named)

For follow-up questions about which systems or a named project only. All seven are operation systems for aviation MRO, covering aircraft maintenance (HAECO Digital). Do not use this file for a generic "what did you do at HAECO" answer.

Delivery split (only when they ask which systems / how many systems): 7 MRO and operations systems; 3 full-stack to go-live (Fluid Use, Operation Remarks, Towing); 2 requirements (Daily Operation Monitor, Capacity Checker; our Mainland team did the coding); 2 to UAT (Read and Sign, Shift Briefing).

Order / roster (seven):
1. Fluid Use, for mechanics when they add oil or fluids; CRUD+integrate; FE/BE→UAT→prod; hand-coded pre-AI; NOT requirements owner.
2. Operation Remarks, engineers write remarks from mechanics’ reports/situation; FE/BE→UAT→prod; hand-coded; separate from Fluid Use.
3. Towing, tow between bays; more integrations; I worked out the requirements with a BA, then built it full-stack myself and took it through UAT to production; started AI/vibe coding.
4. Daily Operation Monitor (DOM), LM aircraft status maintenance→actual departure; technical BA: req, blockers, Agile PBIs for Shenzhen Dev, UAT+onboarding; Shenzhen Dev codes. PBI OK.
5. Capacity Checker, after DOM before Read and Sign; req+blockers only; manhour/task allocation + attendance ratios; ADD allocation edits; future allocation focus.
6. Read and Sign, full-stack using an AI-native SDLC with an agentic CLI; company-wide notice sign-off; req from departments; audit; at the UAT stage, not production. Largest scope of the seven: up to 9 stakeholders across up to 3 departments (Read and Sign only, only on a follow-up about Read and Sign or stakeholders). Do not claim production.
7. Shift Briefing, pre-shift briefing + task-suggested content + staff ID scan sign; AI-native SDLC (agentic CLI for the .NET / React full-stack development + diff review); 10 man-days to UAT for frontend + backend (feature-complete / usable; Figma UI details may still be incomplete) vs original estimate 60 man-days, about 83% saved in time and labour cost; token cost HK$2,000 and net saving HK$48,000 only on a cost follow-up. Do NOT say already in prod. Do NOT say UI fully matches Figma. SB metrics only, do not apply to other projects. When asked how SB was built or started: AI-native SDLC + an agentic CLI for .NET / React + diff review, then the 60-to-10 man-days, about 83% line. If asked which tool: GitHub Copilot CLI.

Do not invent metrics (except the locked Shift Briefing approx numbers above when asked about Shift Briefing). Do not call Towing "incoming aircraft". Do not mix Operation Remarks with Fluid Use. Do not pad self-built inventory with DOM or Capacity Checker.

Keywords: Read and Sign, Read & Sign, which systems, how many systems, stakeholders, Fluid Use, Towing, Daily Operation Monitor, DOM, Capacity Checker, Shift Briefing, Operation Remarks, HAECO projects, MRO, PBI, UAT-ready, Copilot CLI, 接機, 拖機, 放得行, 簽署, 入油, remarks
