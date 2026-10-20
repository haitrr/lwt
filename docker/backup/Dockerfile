# The backup sidecar, the same one haitrr/life runs. Dumps are encrypted with age
# before they reach the volume, so no plaintext copy ever lands on the NAS.
#
# postgres:17 supplies a pg_dump that matches the server; bash is here for
# `set -o pipefail`, which turns a failed pg_dump into a failed backup rather
# than an encrypted empty file.
FROM postgres:17-alpine

RUN apk add --no-cache age bash

COPY dump.sh loop.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/dump.sh /usr/local/bin/loop.sh

# The base image's entrypoint would try to initialise a database cluster.
ENTRYPOINT ["/usr/local/bin/loop.sh"]
