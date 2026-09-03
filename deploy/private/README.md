# Private deploy overlays

`current-package.md` is gitignored. Keep the real file only on the host (not in this public repo).

Example path on the VPS: `/home/admin/interviewme-private/current-package.md`
Mount into the container at `/app/knowledge/facts/current-package.md` if the live bot should answer current-pay questions.
