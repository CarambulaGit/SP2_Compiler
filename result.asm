calculateLCM:
        push    rbp
        mov     rbp, rsp
        mov     DWORD PTR [rbp-4], edi
        mov     DWORD PTR [rbp-8], esi
        mov     DWORD PTR [rbp-12], edx
        mov     eax, DWORD PTR [rbp-4]
        imul    eax, DWORD PTR [rbp-8]
        cdq
        idiv    DWORD PTR [rbp-12]
        pop     rbp
        ret
calculateGCD:
        push    rbp
        mov     rbp, rsp
        mov     DWORD PTR [rbp-4], edi
        mov     DWORD PTR [rbp-8], esi
        jmp     .L4
.L6:
        mov     eax, DWORD PTR [rbp-4]
        cmp     eax, DWORD PTR [rbp-8]
        jle     .L5
        mov     eax, DWORD PTR [rbp-8]
        sub     DWORD PTR [rbp-4], eax
        jmp     .L4
.L5:
        mov     eax, DWORD PTR [rbp-4]
        sub     DWORD PTR [rbp-8], eax
.L4:
        mov     eax, DWORD PTR [rbp-4]
        cmp     eax, DWORD PTR [rbp-8]
        jne     .L6
        mov     eax, DWORD PTR [rbp-8]
        pop     rbp
        ret
main:
        push    rbp
        mov     rbp, rsp
        sub     rsp, 16
        mov     esi, 20
        mov     edi, 5
        call    calculateGCD
        mov     DWORD PTR [rbp-4], eax
        mov     eax, DWORD PTR [rbp-4]
        mov     edx, eax
        mov     esi, 20
        mov     edi, 5
        call    calculateLCM
        mov     DWORD PTR [rbp-8], eax
        mov     eax, 0
        leave
        ret