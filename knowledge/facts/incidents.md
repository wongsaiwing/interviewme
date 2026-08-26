# Production incidents and after-hours ownership

Interview answers from Silas Wong, 2026-08-26. Facts only. Do not invent other outages, rollbacks, or monitoring stories.

Keywords: incident, hotfix, production, data streaming, pipeline, UAT, edge case, Operations, after hours, Shenzhen, 事故, 收工

## Typical data / streaming incidents

Problems are usually on the data side. Example: an aircraft that needs to be received urgently, the data has come in, but nobody goes to pick the aircraft up. That is often because upstream data streaming failed. In those cases I often have to contact the data team to repair the pipeline.

Keywords: urgent aircraft, reception, nobody picking up, data streaming, pipeline, data team, 急機, 接機, 數據

## Shenzhen delivery partner

Besides pairing with AI to write code, I also work with a Shenzhen team as a partner. I give them requirements; they take the work and implement it in a structured way, including the harder / edge pieces.

Keywords: Shenzhen, 深圳, partner, 夾方, requirements, implementation

## Typo / edge-case missed in UAT

There was a case where the Shenzhen team made a typo. It was edge-case handling, so UAT testing did not catch it. That led to a data problem: a specific case could not be handled. It became a hotfix at the highest priority and affected Operations. Because of hotfixes like this, I still have to stay responsible for the project after work hours.

Keywords: typo, edge case, UAT miss, hotfix, highest priority, Operations, after hours, 打錯字, 收工
