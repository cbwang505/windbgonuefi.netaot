
EXTERN vtl_call_fn : QWORD
EXTERN vtl_ret_fn : QWORD
EXTERN vtl_ret_fn_vtl1 : QWORD
EXTERN signalflag : QWORD
EXTERN signalvalue : QWORD
EXTERN hv_vtl_ap_entry : PROC
EXTERN SkpPrepareForReturnToNormalMode : PROC
EXTERN hv_acquire_hypercall_input_page : PROC
EXTERN hv_acquire_hypercall_output_page : PROC
EXTERN ProcessSynicChannel : PROC
EXTERN VmbusSintIdtWindbgEntry : PROC
EXTERN HvApicSelfIpiVtl0 : PROC
EXTERN dumpbuf : PROC
EXTERN DumpRspFunction : PROC
EXTERN UefiMemoryStackTrace : PROC


EXTERN FindRuntimeFunction : PROC
EXTERN CheckRuntimeFunction : PROC

EXTERN InitGlobalHvVtl1 : PROC
EXTERN HvcallCodeVa : QWORD
EXTERN HvcallCodeVaVTL1 : QWORD
EXTERN SkiInitialMxCsr : DWORD

.CODE






mPUSHAD MACRO
	push rax
	push rbx
	push rcx
	push rdx
	push rdi
	push rsi
	push r8
	push r9
	push r10
	push r11
	push r12
	push r13
	push r14
	push r15
ENDM

mPOPAD MACRO
	pop r15
	pop r14
	pop r13
	pop r12
	pop r11
	pop r10
	pop r9
	pop r8
	pop rsi
	pop rdi
	pop rdx
	pop rcx
	pop rbx
	pop rax
ENDM





HVHyperCall PROC

 
push rdi
push rdx                                ; Store output PCPU_REG_64

mov rsi, rcx

;
; Hypercall inputs
; RCX = Hypercall input value
; RDX = Input param GPA
; R8  = Output param GPA 
;
mov rcx, qword ptr [rsi+0h]
mov rdx, qword ptr [rsi+8h]
mov r8,  qword ptr [rsi+10h]

    
;
; Extended fast hypercall (set it regardless)
;
EXT_HYPERCALL_XMM_SETUP:
movups xmm0, xmmword ptr[rsi+18h]
movups xmm1, xmmword ptr[rsi+28h]
movups xmm2, xmmword ptr[rsi+38h]
movups xmm3, xmmword ptr[rsi+48h]
movups xmm4, xmmword ptr[rsi+58h]
movups xmm5, xmmword ptr [rsi+68h]
push rsi

MAKE_VMCALL:
;int 3
vmcall

pop rsi
mov     qword ptr [rsi+0h], rdx
mov     qword ptr [rsi+8h], r8
movups  xmmword ptr [rsi+10h], xmm0
movups  xmmword ptr [rsi+20h], xmm1
movups  xmmword ptr [rsi+30h], xmm2
movups  xmmword ptr [rsi+40h], xmm3
movups  xmmword ptr [rsi+50h], xmm4
movups  xmmword ptr [rsi+60h], xmm5

pop rdx
pop rdi
  
;
; RAX from vmcall is return code for our subroutine too
;
ret
HVHyperCall ENDP

HVHyperCallHvCallInstallInterceptAsm PROC
push rdi
push rdx                                ; Store output PCPU_REG_64
push rsi                                ; Store output PCPU_REG_64
sub rsp ,100h

call InitGlobalHvVtl1

call hv_acquire_hypercall_input_page
push rax
call hv_acquire_hypercall_output_page
mov r8,rax
pop rdx
mov rcx,-1
mov qword ptr [rdx+0h],rcx
mov rcx,0b00000002h
mov qword ptr [rdx+8h],rcx
mov rcx,2h
mov qword ptr [rdx+10h],rcx
mov rcx, 004dh
call HvcallCodeVaVTL1
call rax
mov rcx,rax
int 29h

add rsp ,100h
pop rsi
pop rdx
pop rdi
  
;
; RAX from vmcall is return code for our subroutine too
;
ret
HVHyperCallHvCallInstallInterceptAsm ENDP



HVHyperCallHvCallInstallIntercept PROC

 
push rdi
push rdx                                ; Store output PCPU_REG_64
push rsi                                ; Store output PCPU_REG_64

sub rsp ,100h
mov rcx,-1
mov qword ptr [rsp+8h],rcx
mov rcx,0b00000002h
mov qword ptr [rsp+10h],rcx
mov rcx,2h
mov qword ptr [rsp+18h],rcx

mov rsi, rsp

;
; Hypercall inputs
; RCX = Hypercall input value
; RDX = Input param GPA
; R8  = Output param GPA 
;
mov rcx, 01004dh
mov rdx, qword ptr [rsi+8h]
mov r8,  qword ptr [rsi+10h]

    
;
; Extended fast hypercall (set it regardless)
;
EXT_HYPERCALL_XMM_SETUP:
movups xmm0, xmmword ptr[rsi+18h]
movups xmm1, xmmword ptr[rsi+28h]
movups xmm2, xmmword ptr[rsi+38h]
movups xmm3, xmmword ptr[rsi+48h]
movups xmm4, xmmword ptr[rsi+58h]
movups xmm5, xmmword ptr [rsi+68h]

xor rax,rax

MAKE_VMCALL:
;int 3
vmcall

hlt

mov     qword ptr [rsi+0h], rdx
mov     qword ptr [rsi+8h], r8
movups  xmmword ptr [rsi+10h], xmm0
movups  xmmword ptr [rsi+20h], xmm1
movups  xmmword ptr [rsi+30h], xmm2
movups  xmmword ptr [rsi+40h], xmm3
movups  xmmword ptr [rsi+50h], xmm4
movups  xmmword ptr [rsi+60h], xmm5

add rsp ,100h
pop rsi
pop rdx
pop rdi
  
;
; RAX from vmcall is return code for our subroutine too
;
ret
HVHyperCallHvCallInstallIntercept ENDP



MAKE_VMCALL:
;int 3
vmcall
;
; RAX from vmcall is return code for our subroutine too
;
ret

CpuSleep PROC

 hlt

CpuSleep ENDP

CpuNOP PROC
mov rcx ,rax
ret
CpuNOP ENDP

HV_VTL_AP_ENTRY_HANDLER PROC
sub rsp ,300
mPUSHAD
call hv_vtl_ap_entry
mPOPAD
add rsp,300
xor     rcx, rcx
mov rax,vtl_ret_fn_vtl1
call rax
jmp CpuSleep
ret

HV_VTL_AP_ENTRY_HANDLER ENDP



ShvlpVtlCall PROC
sub     rsp, 138h
lea     rax, [rsp+100h]
movups  xmmword ptr [rsp+30h], xmm6
movups  xmmword ptr [rsp+40h], xmm7
movups  xmmword ptr [rsp+50h], xmm8
movups  xmmword ptr [rsp+60h], xmm9
movups  xmmword ptr [rsp+70h], xmm10
movups  xmmword ptr [rax-80h], xmm11
movups  xmmword ptr [rax-70h], xmm12
movups  xmmword ptr [rax-60h], xmm13
movups  xmmword ptr [rax-50h], xmm14
movups  xmmword ptr [rax-40h], xmm15
mov     [rax-8], rbp
mov     [rax], rbx
mov     [rax+8], rdi
mov     [rax+10h], rsi
mov     [rax+18h], r12
mov     [rax+20h], r13
mov     [rax+28h], r14
mov     [rax+30h], r15
mov rbx,rsp
mov     rax, vtl_call_fn
xor     rcx, rcx
call   rax
mov rcx ,rax
call SkpPrepareForReturnToNormalMode
call HvApicSelfIpiVtl0
hlt
jmp CpuSleep
lea     rcx, [rsp+100h]
movups  xmm6, xmmword ptr [rsp+30h]
movups  xmm7, xmmword ptr [rsp+40h]
movups  xmm8, xmmword ptr [rsp+50h]
movups  xmm9, xmmword ptr [rsp+60h]
movups  xmm10, xmmword ptr [rsp+70h]
movups  xmm11, xmmword ptr [rcx-80h]
movups  xmm12, xmmword ptr [rcx-70h]
movups  xmm13, xmmword ptr [rcx-60h]
movups  xmm14, xmmword ptr [rcx-50h]
movups  xmm15, xmmword ptr [rcx-40h]
mov     rbx, [rcx]
mov     rdi, [rcx+8]
mov     rsi, [rcx+10h]
mov     r12, [rcx+18h]
mov     r13, [rcx+20h]
mov     r14, [rcx+28h]
mov     r15, [rcx+30h]
mov     rbp, [rcx-8]
add     rsp, 138h
ret
ShvlpVtlCall ENDP




hdlmsgint PROC
mov signalflag,1

sub     rsp, 32 + 8   
call ProcessSynicChannel
add     rsp, 32 + 8    

IRETQ 
hdlmsgint ENDP




EnableInterrupts PROC
sti
ret
EnableInterrupts ENDP

DisableInterrupts PROC
cli
ret
DisableInterrupts ENDP

DumpRsp PROC
mov rcx,rsp
sub     rsp, 32 + 8
call DumpRspFunction
add     rsp, 32 + 8  
ret
DumpRsp ENDP


DumpThis PROC
mov rdx,rsp
call $+5
pop rcx
sub     rsp, 32 + 8
call UefiMemoryStackTrace
add     rsp, 32 + 8  
ret
DumpThis ENDP

DebugBreak PROC
int 3
ret
DebugBreak ENDP







CallExceptionHandler PROC
mov rcx,rsp
retry:
add  rcx,8
mov rsi,[rcx]
test rsi,rsi
je retry
push rcx
mov rcx,rsi
sub rsp,40h
call FindRuntimeFunction
add rsp,40h
test rax,rax
pop rcx
je retry

push rax
retrynext:
add  rcx,8
mov rsi,[rcx]
test rsi,rsi
je retrynext
push rcx
mov rcx,rsi
sub rsp,40h
call CheckRuntimeFunction
add rsp,40h
pop rcx
test rax,rax
je retrynext
pop rax
mov rsp,rcx
jmp rax
ret
CallExceptionHandler ENDP





CallExceptionHandlerIdt PROC
retry:
add  rcx,8
mov rsi,[rcx]
test rsi,rsi
je retry
push rcx
mov rcx,rsi
sub rsp,40h
call FindRuntimeFunction
add rsp,40h
test rax,rax
pop rcx
je retry
push rcx
sub rsp,40h
call rax
add rsp,40h
pop rcx
add  rcx,8
mov rax,rcx
ret
CallExceptionHandlerIdt ENDP

 RhpStackProbe PROC
        ; On entry:
        ;   r11 - points to the lowest address on the stack frame being allocated (i.e. [InitialSp - FrameSize])
        ;   rsp - points to some byte on the last probed page
        ; On exit:
        ;   rax - is not preserved
        ;   r11 - is preserved
        ;
        ; NOTE: this helper will probe at least one page below the one pointed by rsp.

        mov     rax, rsp               ; rax points to some byte on the last probed page
        and     rax, -1000h       ; rax points to the **lowest address** on the last probed page
                                       ; This is done to make the following loop end condition simpler.

ProbeLoop:
        sub     rax, 1000h         ; rax points to the lowest address of the **next page** to probe
        test    dword ptr [rax], eax   ; rax points to the lowest address on the **last probed** page
        cmp     rax, r11
        jg      ProbeLoop              ; If (rax > r11), then we need to probe at least one more page.

        ret
 RhpStackProbe ENDP

END
