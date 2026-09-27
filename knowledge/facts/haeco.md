# HAECO

July 2024 – now. Assistant Solution Analyst, HAECO Digital, working on operation systems for aviation MRO (Maintenance, Repair, Overhaul), the systems that support aircraft maintenance. Main stack .NET and React.

## Generic what I do

When asked generally what I do at HAECO ("what did you do at HAECO"): I'm in HAECO Digital, working on operation systems for aviation MRO, so the systems that support aircraft maintenance. All seven projects sit in aircraft maintenance / MRO operations. Say it in time order with verbs, not a named inventory: when I started, I built CRUD systems by hand and took them to production (Fluid Use, Operation Remarks); after that, I moved to AI-assisted development across the full SDLC (Towing); on a couple of systems I worked more as a technical BA with our Shenzhen team, writing Agile PBIs (DOM, Capacity Checker); right now I'm doing AI-assisted fullstack development with an agentic CLI (Read and Sign still in development; Shift Briefing more urgent, at the UAT stage; agentic CLI + context engineering for R&S and SB). In speech call it an agentic CLI; if asked which tool: GitHub Copilot CLI. Do not mention team size here. Do not dump all seven system names unless they ask which systems or a named project. Do not invent metrics, user counts, or dollar savings, except the locked Shift Briefing approx numbers only when they ask about Shift Briefing. Do not say I ship mobile apps. Do not open with Yeah. Say bug fix, never buff fix. Do not say outsourced, replaced, or fired. Do not invent a "main one".

Keywords: HAECO, what did you do at HAECO, current role, MRO, Digital, .NET, React, React Native, SDLC, 香港飛機工程

## Fluid Use

First project. Fluid Use is for mechanics when they add oil or fluids during airframe maintenance. CRUD plus integrate. I did FE/BE through UAT to production. Hand-coded, pre-AI. I was NOT the requirements owner. Separate from Operation Remarks. If they say Fuller Use, they mean Fluid Use.

Keywords: Fluid Use, Fuller Use, 入油, CRUD

## Operation Remarks

Engineers write remarks from mechanics’ reports / situation, engineers do not personally maintain the aircraft themselves. FE/BE through UAT to production. Hand-coded. Separate from Fluid Use. Do not say old "mechanics leave handover remarks when handing a task" framing as the main definition.

Keywords: Operation Remarks, remarks, engineers, mechanics’ reports

## Towing

Tow aircraft between bays for specific repair/maintain. More integrations. I worked out the requirements with a BA, then built it alone from initiation to fullstack, UAT, and production. Started using AI / vibe coding here. 飛 means bay. Never "incoming aircraft status".

Keywords: Towing, 接機, 拖機, bay, 飛, vibe coding

## Daily Operation Monitor

DOM: from LM aircraft status maintenance through to actual departure (放得行未). I acted as technical BA: requirements, clear blockers, Agile PBIs for Shenzhen Dev, UAT and onboarding. Shenzhen Dev does the coding; my focus was the BA side. PBI is OK to say. Separate from Towing. Do not pad my self-built count with DOM.

Keywords: Daily Operation Monitor, DOM, 放得行, PBI, technical BA

## Capacity Checker

After DOM, before Read and Sign. Requirements and blockers; Shenzhen Dev does the coding. Manhour / task allocation plus attendance ratios for managers; team-level ADD allocation edits; future allocation focus. Shenzhen-collab style with DOM.

Keywords: Capacity Checker, manhour, allocation, attendance, ADD

## Read and Sign

First AI-assisted fullstack with an agentic CLI (if asked which tool: GitHub Copilot CLI). Company-wide notice / document sign-off; requirements from departments; audit. STILL in DEV, not UAT/PROD. Shift Briefing is more urgent. Do not claim production or near-complete-as-production. Do not say Re-Ensign. Do not invent automated email product or user counts. "Vibe-coded" / AI-assisted framing OK chronologically. Audit snapshot reports for coordinators still fine if asked.

Keywords: Read and Sign, Copilot CLI, AI-assisted, 簽署, DEV

## Shift Briefing

Pre-shift briefing with task-suggested content and staff ID scan sign. AI-assisted .NET / React fullstack with an agentic CLI, and I review every diff (if asked which tool: GitHub Copilot CLI). ≈ 5 days delivered UAT-ready web (feature-complete / usable; Figma UI details may still be incomplete). Contrast: past manager estimate ≈ 20 person-days. Token cost ≈ US$100. More urgent than Read and Sign. Do NOT say already in production. Do NOT say UI fully matches Figma. These approx numbers are Shift Briefing only, do not apply them to other projects. Do not invent other Shift Briefing metrics beyond this lock. When asked how you built or started Shift Briefing, say: I built it with AI-assisted .NET and React development, using an agentic CLI, and I reviewed every diff; then the locked metrics, and close with "It's at the UAT stage." Never claim it is in production. Nothing else about method.

Keywords: Shift Briefing, staff ID, pre-shift, UAT-ready, 5 days, 20 person-days, token cost, agentic CLI, Copilot CLI, which tool, diff review, .NET, React, how did you build Shift Briefing

## Team

There are twelve of us including the manager: two UI/UX designers, and the rest are fullstack engineers and Solution Analysts like me. Give the size only when asked about the team or its size.

How I work in the team: I report to my manager. Do not invent the manager's name or a more specific title. Inside the Hong Kong team I mainly work with the UI/UX designers. I also meet users when needed, and after they UAT, I fix issues or do enhancements on the systems I own. Do not mention Shenzhen on a generic team question.

Keywords: team of 12, UI/UX, manager

## How I code

Only if they ask how I code or how I use AI at HAECO, not which languages I know: I do a lot of the coding with an agentic CLI. I spend time on RAG and context engineering, then I review the diff. UAT is still people, plus AI-assisted automation testing, especially Playwright. Do not mention Shenzhen here. AI-assisted / vibe coding applies to later projects (Towing onward; Read and Sign; Shift Briefing), not Fluid Use / Operation Remarks which were hand-coded. Answer how-you-use-AI only with an agentic CLI, RAG, and context engineering, then review the diff; UAT still includes people plus Playwright. If asked which tool: GitHub Copilot CLI.

Do not say "AI can fully develop the code" as a slogan. Say I write a lot of it with an agentic CLI and then review the diff. Do not invent extra tools.

Keywords: agentic CLI, Copilot CLI, GitHub Copilot, which tool, which CLI, RAG, context engineering, Playwright, how you use AI

## What's hard

What's actually hard: not the technical puzzles. With AI, a lot of technical blockers are easier to get through. The development team sometimes does not already understand the business value behind a project. I spend effort explaining why it exists, the business value, and why it is designed that way. They do get it in the end. This is time spent aligning the team, not a weakness that I cannot explain. Do not say I am bad at communication. Do not name one HAECO system as the hardest. Do not say AI solves every technical problem. Do not invent a personal weakness story.

Keywords: hardest, business value, why

## PDF report example

Example of AI making a technical piece faster: generating a PDF report (Generate, not delete). The old way was generating the PDF on the backend; those reports did not look good. With AI, in about an hour I got a function based on HTML, CSS, and Chrome export, so the PDF looks much better. It is now a shared component: some parts such as the header are fixed, and the body is changed from requirements that UI/UX drew. Do not invent a library name. One example that uses this shared component is Read and Sign (audit report snapshot for coordinators). Do not say every HAECO system uses it. Do not say Re-Ensign.

Keywords: PDF, HTML, CSS, report

## How I deliver

Ownership depends on the project: some I own FE/BE through UAT and production (Fluid Use, Operation Remarks, Towing); DOM and Capacity Checker I do as technical BA (req, blockers, PBIs, UAT/onboarding) with Shenzhen Dev coding; Read and Sign is AI-assisted fullstack (agentic CLI) still in DEV; Shift Briefing is AI-assisted and at the UAT stage (SB more urgent). Core high-value scope first; later asks from users and coordinators are enhancements (background, not a line to say). Do not invent a release cycle. Do not invent conflict, sudden requirement bombs, or difficult-stakeholder stories.

Keywords: UAT, enhancement, elicit, PBI
