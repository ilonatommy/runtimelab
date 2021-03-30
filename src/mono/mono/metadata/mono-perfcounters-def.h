/**
 * \file
 * Define the system and runtime performance counters.
 * Each category is defined with the macro:
 * 	PERFCTR_CAT(catid, name, help, type, instances, first_counter_id)
 * and after that follows the counters inside the category, defined by the macro:
 * 	PERFCTR_COUNTER(counter_id, name, help, type, field)
 * field is the field inside MonoPerfCounters per predefined counters.
 * Note we set it to unused for unrelated counters: it is unused
 * in those cases.
 */
PERFCTR_CAT(CPU, "Processor", "", MultiInstance, CPU, CPU_USER_TIME)
PERFCTR_COUNTER(CPU_USER_TIME, "% User Time", "", Timer100Ns, unused)
PERFCTR_COUNTER(CPU_PRIV_TIME, "% Privileged Time", "", Timer100Ns, unused)
PERFCTR_COUNTER(CPU_INTR_TIME, "% Interrupt Time", "", Timer100Ns, unused)
PERFCTR_COUNTER(CPU_DCP_TIME,  "% DCP Time", "", Timer100Ns, unused)
PERFCTR_COUNTER(CPU_PROC_TIME, "% Processor Time", "", Timer100NsInverse, unused)

PERFCTR_CAT(PROC, "Process", "", MultiInstance, Process, PROC_USER_TIME)
PERFCTR_COUNTER(PROC_USER_TIME, "% User Time", "", Timer100Ns, unused)
PERFCTR_COUNTER(PROC_PRIV_TIME, "% Privileged Time", "", Timer100Ns, unused)
PERFCTR_COUNTER(PROC_PROC_TIME, "% Processor Time", "", Timer100Ns, unused)
PERFCTR_COUNTER(PROC_THREADS,   "Thread Count", "", NumberOfItems64, unused)
PERFCTR_COUNTER(PROC_VBYTES,    "Virtual Bytes", "", NumberOfItems64, unused)
PERFCTR_COUNTER(PROC_WSET,      "Working Set", "", NumberOfItems64, unused)
PERFCTR_COUNTER(PROC_PBYTES,    "Private Bytes", "", NumberOfItems64, unused)

/* sample runtime counter */
PERFCTR_CAT(MONO_MEM, "Mono Memory", "", SingleInstance, Mono, MEM_NUM_OBJECTS)
PERFCTR_COUNTER(MEM_NUM_OBJECTS, "Allocated Objects", "", NumberOfItems64, unused)
PERFCTR_COUNTER(MEM_PHYS_TOTAL, "Total Physical Memory", "Physical memory installed in the machine, in bytes", NumberOfItems64, unused)
PERFCTR_COUNTER(MEM_PHYS_AVAILABLE, "Available Physical Memory", "Physical memory available in the machine, in bytes", NumberOfItems64, unused)

PERFCTR_CAT(ASPNET, "ASP.NET", "", MultiInstance, Mono, ASPNET_REQ_Q)
PERFCTR_COUNTER(ASPNET_REQ_Q, "Requests Queued", "", NumberOfItems64, aspnet_requests_queued)
PERFCTR_COUNTER(ASPNET_REQ_TOTAL, "Requests Total", "", NumberOfItems32, aspnet_requests)
PERFCTR_COUNTER(ASPNET_REQ_PSEC, "Requests/Sec", "", RateOfCountsPerSecond32, aspnet_requests)

PERFCTR_CAT(JIT, ".NET CLR JIT", "", MultiInstance, Mono, JIT_BYTES)
PERFCTR_COUNTER(JIT_BYTES, "# of IL Bytes JITted", "", NumberOfItems32, jit_bytes)
PERFCTR_COUNTER(JIT_METHODS, "# of IL Methods JITted", "", NumberOfItems32, jit_methods)
PERFCTR_COUNTER(JIT_TIME, "% Time in JIT", "", RawFraction, jit_time)
PERFCTR_COUNTER(JIT_BYTES_PSEC, "IL Bytes Jitted/Sec", "", RateOfCountsPerSecond32, jit_bytes)
PERFCTR_COUNTER(JIT_FAILURES, "Standard Jit Failures", "", NumberOfItems32, jit_failures)

PERFCTR_CAT(EXC, ".NET CLR Exceptions", "", MultiInstance, Mono, EXC_THROWN)
PERFCTR_COUNTER(EXC_THROWN, "# of Exceps Thrown", "", NumberOfItems32, exceptions_thrown)
PERFCTR_COUNTER(EXC_THROWN_PSEC, "# of Exceps Thrown/Sec", "", RateOfCountsPerSecond32, exceptions_thrown)
PERFCTR_COUNTER(EXC_FILTERS_PSEC, "# of Filters/Sec", "", RateOfCountsPerSecond32, exceptions_filters)
PERFCTR_COUNTER(EXC_FINALLYS_PSEC, "# of Finallys/Sec", "", RateOfCountsPerSecond32, exceptions_finallys)
PERFCTR_COUNTER(EXC_CATCH_DEPTH, "Throw to Catch Depth/Sec", "", NumberOfItems32, exceptions_depth)

PERFCTR_CAT(GC, ".NET CLR Memory", "", MultiInstance, Mono, GC_GEN0)
PERFCTR_COUNTER(GC_GEN0, "# Gen 0 Collections", "", NumberOfItems32, gc_collections0)
PERFCTR_COUNTER(GC_GEN1, "# Gen 1 Collections", "", NumberOfItems32, gc_collections1)
PERFCTR_COUNTER(GC_GEN2, "# Gen 2 Collections", "", NumberOfItems32, gc_collections2)
PERFCTR_COUNTER(GC_PROM0, "Promoted Memory from Gen 0", "", NumberOfItems32, gc_promotions0)
PERFCTR_COUNTER(GC_PROM1, "Promoted Memory from Gen 1", "", NumberOfItems32, gc_promotions1)
PERFCTR_COUNTER(GC_PROM0SEC, "Gen 0 Promoted Bytes/Sec", "", RateOfCountsPerSecond32, gc_promotions0)
PERFCTR_COUNTER(GC_PROM1SEC, "Gen 1 Promoted Bytes/Sec", "", RateOfCountsPerSecond32, gc_promotions1)
PERFCTR_COUNTER(GC_PROMFIN, "Promoted Finalization-Memory from Gen 0", "", NumberOfItems32, gc_promotion_finalizers)
PERFCTR_COUNTER(GC_GEN0SIZE, "Gen 0 heap size", "", NumberOfItems64, gc_gen0size)
PERFCTR_COUNTER(GC_GEN1SIZE, "Gen 1 heap size", "", NumberOfItems64, gc_gen1size)
PERFCTR_COUNTER(GC_GEN2SIZE, "Gen 2 heap size", "", NumberOfItems64, gc_gen2size)
PERFCTR_COUNTER(GC_LOSIZE, "Large Object Heap size", "", NumberOfItems32, gc_lossize)
PERFCTR_COUNTER(GC_FINSURV, "Finalization Survivors", "", NumberOfItems32, gc_fin_survivors)
PERFCTR_COUNTER(GC_NHANDLES, "# GC Handles", "", NumberOfItems32, gc_num_handles)
PERFCTR_COUNTER(GC_BYTESSEC, "Allocated Bytes/sec", "", RateOfCountsPerSecond32, gc_allocated)
PERFCTR_COUNTER(GC_INDGC, "# Induced GC", "", NumberOfItems32, gc_induced)
PERFCTR_COUNTER(GC_PERCTIME, "% Time in GC", "", RawFraction, gc_time)
PERFCTR_COUNTER(GC_BYTES, "# Bytes in all Heaps", "", NumberOfItems64, gc_total_bytes)
PERFCTR_COUNTER(GC_COMMBYTES, "# Total committed Bytes", "", NumberOfItems64, gc_committed_bytes)
PERFCTR_COUNTER(GC_RESBYTES, "# Total reserved Bytes", "", NumberOfItems64, gc_reserved_bytes)
PERFCTR_COUNTER(GC_PINNED, "# of Pinned Objects", "", NumberOfItems32, gc_num_pinned)
PERFCTR_COUNTER(GC_SYNKB, "# of Sink Blocks in use", "", NumberOfItems32, gc_sync_blocks)

PERFCTR_CAT(LOADING, ".NET CLR Loading", "", MultiInstance, Mono, LOADING_CLASSES)
PERFCTR_COUNTER(LOADING_CLASSES, "Current Classes Loaded", "", NumberOfItems32, loader_classes)
PERFCTR_COUNTER(LOADING_TOTCLASSES, "Total Classes Loaded", "", NumberOfItems32, loader_total_classes)
PERFCTR_COUNTER(LOADING_CLASSESSEC, "Rate of Classes Loaded", "", RateOfCountsPerSecond32, loader_total_classes)
PERFCTR_COUNTER(LOADING_APPDOMAINS, "Current appdomains", "", NumberOfItems32, loader_appdomains)
PERFCTR_COUNTER(LOADING_TOTAPPDOMAINS, "Total Appdomains", "", NumberOfItems32, loader_total_appdomains)
PERFCTR_COUNTER(LOADING_APPDOMAINSEC, "Rate of appdomains", "", RateOfCountsPerSecond32, loader_total_appdomains)
PERFCTR_COUNTER(LOADING_ASSEMBLIES, "Current Assemblies", "", NumberOfItems32, loader_assemblies)
PERFCTR_COUNTER(LOADING_TOTASSEMBLIES, "Total Assemblies", "", NumberOfItems32, loader_total_assemblies)
PERFCTR_COUNTER(LOADING_ASSEMBLIESEC, "Rate of Assemblies", "", RateOfCountsPerSecond32, loader_total_assemblies)
PERFCTR_COUNTER(LOADING_FAILURES, "Total # of Load Failures", "", NumberOfItems32, loader_failures)
PERFCTR_COUNTER(LOADING_FAILURESSEC, "Rate of Load Failures", "", RateOfCountsPerSecond32, loader_failures)
PERFCTR_COUNTER(LOADING_BYTES, "Bytes in Loader Heap", "", NumberOfItems32, loader_bytes)
PERFCTR_COUNTER(LOADING_APPUNLOADED, "Total appdomains unloaded", "", NumberOfItems32, loader_appdomains_uloaded)
PERFCTR_COUNTER(LOADING_APPUNLOADEDSEC, "Rate of appdomains unloaded", "", RateOfCountsPerSecond32, loader_appdomains_uloaded)

PERFCTR_CAT(THREAD, ".NET CLR LocksAndThreads", "", MultiInstance, Mono, THREAD_CONTENTIONS)
PERFCTR_COUNTER(THREAD_CONTENTIONS, "Total # of Contentions", "", NumberOfItems32, thread_contentions)
PERFCTR_COUNTER(THREAD_CONTENTIONSSEC, "Contention Rate / sec", "", RateOfCountsPerSecond32, thread_contentions)
PERFCTR_COUNTER(THREAD_QUEUELEN, "Current Queue Length", "", NumberOfItems32, thread_queue_len)
PERFCTR_COUNTER(THREAD_QUEUELENP, "Queue Length Peak", "", NumberOfItems32, thread_queue_max)
PERFCTR_COUNTER(THREAD_QUEUELENSEC, "Queue Length / sec", "", RateOfCountsPerSecond32, thread_queue_max)
PERFCTR_COUNTER(THREAD_NUMLOG, "# of current logical Threads", "", NumberOfItems32, thread_num_logical)
PERFCTR_COUNTER(THREAD_NUMPHYS, "# of current physical Threads", "", NumberOfItems32, thread_num_physical)
PERFCTR_COUNTER(THREAD_NUMREC, "# of current recognized threads", "", NumberOfItems32, thread_cur_recognized)
PERFCTR_COUNTER(THREAD_TOTREC, "# of total recognized threads", "", NumberOfItems32, thread_num_recognized)
PERFCTR_COUNTER(THREAD_TOTRECSEC, "rate of recognized threads / sec", "", RateOfCountsPerSecond32, thread_num_recognized)

PERFCTR_CAT(INTEROP, ".NET CLR Interop", "", MultiInstance, Mono, INTEROP_NUMCCW)
PERFCTR_COUNTER(INTEROP_NUMCCW, "# of CCWs", "", NumberOfItems32, interop_num_ccw)
PERFCTR_COUNTER(INTEROP_STUBS, "# of Stubs", "", NumberOfItems32, interop_num_stubs)
PERFCTR_COUNTER(INTEROP_MARSH, "# of marshalling", "", NumberOfItems32, interop_num_marshals)

PERFCTR_CAT(SECURITY, ".NET CLR Security", "", MultiInstance, Mono, SECURITY_CHECKS)
PERFCTR_COUNTER(SECURITY_CHECKS, "Total Runtime Checks", "", NumberOfItems32, security_num_checks)
PERFCTR_COUNTER(SECURITY_LCHECKS, "# Link Time Checks", "", NumberOfItems32, security_num_link_checks)
PERFCTR_COUNTER(SECURITY_PERCTIME, "% Time in RT checks", "", RawFraction, security_time)
PERFCTR_COUNTER(SECURITY_SWDEPTH, "Stack Walk Depth", "", NumberOfItems32, security_depth)

PERFCTR_CAT(THREADPOOL, "Mono Threadpool", "", MultiInstance, Mono, THREADPOOL_WORKITEMS)
PERFCTR_COUNTER(THREADPOOL_WORKITEMS, "Work Items Added", "", NumberOfItems64, threadpool_workitems)
PERFCTR_COUNTER(THREADPOOL_WORKITEMS_PSEC, "Work Items Added/Sec", "", RateOfCountsPerSecond32, threadpool_workitems)
PERFCTR_COUNTER(THREADPOOL_IOWORKITEMS, "IO Work Items Added", "", NumberOfItems64, threadpool_ioworkitems)
PERFCTR_COUNTER(THREADPOOL_IOWORKITEMS_PSEC, "IO Work Items Added/Sec", "", RateOfCountsPerSecond32, threadpool_ioworkitems)
PERFCTR_COUNTER(THREADPOOL_THREADS, "# of Threads", "", NumberOfItems32, threadpool_threads)
PERFCTR_COUNTER(THREADPOOL_IOTHREADS, "# of IO Threads", "", NumberOfItems32, threadpool_iothreads)

PERFCTR_CAT(NETWORK, "Network Interface", "", MultiInstance, NetworkInterface, NETWORK_BYTESRECSEC)
PERFCTR_COUNTER(NETWORK_BYTESRECSEC, "Bytes Received/sec", "", RateOfCountsPerSecond64, unused)
PERFCTR_COUNTER(NETWORK_BYTESSENTSEC, "Bytes Sent/sec", "", RateOfCountsPerSecond64, unused)
PERFCTR_COUNTER(NETWORK_BYTESTOTALSEC, "Bytes Total/sec", "", RateOfCountsPerSecond64, unused)
