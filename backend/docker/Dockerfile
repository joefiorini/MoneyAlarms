FROM joefiorini/dockerskeleton:mono

ADD stage/MoneyAlarms.Dispatch.* /action/

ADD exec /action/exec
RUN chmod +x /action/exec

CMD ["/bin/bash", "-c", "cd actionProxy && python -u actionproxy.py"]