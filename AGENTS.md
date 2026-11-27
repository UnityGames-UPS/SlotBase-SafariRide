
# Safari Ride — Development Rules

- Inspect the target script and its direct callers before editing.
- Never overwrite serialized Inspector values at runtime to avoid setup work.
- Never auto-find, auto-create, auto-enable, randomize, clamp or substitute required scene references.
- If Inspector setup is missing, report the exact GameObject, component, field and assignment required.
- Do not add unnecessary null checks, fallbacks, try/catch blocks or duplicate state.
- Validate external server data at its boundary. Keep internal code straightforward.
- Backend owns triggers, rewards, payouts, awarded picks/spins and final results.
- Client owns presentation, animation and player input.
- Do not preserve CNY behavior unless Safari Ride explicitly requires it.
- Make the smallest change possible. Avoid unrelated refactoring.
- Preserve serialized field names and existing prefab/scene contracts.
- Never invent backend fields, symbol IDs, feature rules or timings.
- Report Inspector changes and tests actually performed.
- Static code inspection is not runtime verification.
