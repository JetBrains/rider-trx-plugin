Maintainer Guide
================

Rotate the PR Token
-------------------
To make PRs in the `.github/workflows/ide-updater.yml`, the CI uses a special token.

To refresh it, follow the steps:
1. Go to the [Fine-grained tokens][github.tokens] settings page on GitHub.
2. Generate a new token named **github.rider-trx-plugin**, scoped to the **rider-trx-plugin** repository, with the following **Repository permissions**:
    - **Contents**: **Read and write**,
    - **Pull requests**: **Read and write**.
3. Paste the token into [the Action Secrets page][github.secrets] as `PR_TOKEN`.

[github.secrets]: https://github.com/JetBrains/rider-trx-plugin/settings/secrets/actions
[github.tokens]: https://github.com/settings/tokens?type=beta
