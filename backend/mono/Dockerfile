FROM joefiorini/dockerskeleton

RUN echo http://nl.alpinelinux.org/alpine/edge/testing >> /etc/apk/repositories && \
    apk add -U --no-cache python2 mono && \
    cert-sync /etc/ssl/certs/ca-certificates.crt
