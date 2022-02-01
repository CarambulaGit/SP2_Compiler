.386
.model flat,stdcall
option casemap:none

include \masm32\include\masm32rt.inc
_main PROTO

calculateLCM PROTO
calculateGCD PROTO
main PROTO

.data
.code
_start:
push ebp
mov ebp, esp
sub esp, 0
invoke  _main
add esp, 0
mov esp, ebp
pop ebp
ret
_main PROC




invoke main
 

printf("\n")
inkey
ret

_main ENDP

calculateLCM PROC
push ebp
mov ebp, esp
sub esp, 0
mov eax, dword ptr[ebp+16] ; gcd
push eax

mov eax, dword ptr[ebp+12] ; n
push eax

mov eax, dword ptr[ebp+8] ; m
push eax

pop eax
pop ecx
imul ecx
push eax

pop eax
pop ebx
xor edx, edx
div ebx
push eax

pop eax
add esp, 0
mov esp, ebp
pop ebp
ret 12

add esp, 0
mov esp, ebp
pop ebp
ret 12

calculateLCM ENDP
calculateGCD PROC
push ebp
mov ebp, esp
sub esp, 0
Loopname0start:
mov eax, dword ptr[ebp+12] ; n
push eax

mov eax, dword ptr[ebp+8] ; m
push eax

pop eax
pop ecx
cmp eax, ecx
mov eax, 0
setne al
push eax
pop eax
cmp eax, 0
je Loopname0end
mov eax, dword ptr[ebp+12] ; n
push eax

mov eax, dword ptr[ebp+8] ; m
push eax

pop eax
pop ecx
cmp ecx, eax
mov eax, 0
setl al
push eax
pop eax
cmp eax, 0
je name1else
mov eax, dword ptr[ebp+12] ; n
push eax

mov eax, dword ptr[ebp+8] ; m
push eax

pop eax
pop ecx
sub eax, ecx
push eax

	pop eax
	mov dword ptr[ebp+8], eax

jmp name1final
name1else:
mov eax, dword ptr[ebp+8] ; m
push eax

mov eax, dword ptr[ebp+12] ; n
push eax

pop eax
pop ecx
sub eax, ecx
push eax

	pop eax
	mov dword ptr[ebp+12], eax

name1final:

jmp Loopname0start
Loopname0end:

mov eax, dword ptr[ebp+12] ; n
push eax

pop eax
add esp, 0
mov esp, ebp
pop ebp
ret 8

add esp, 0
mov esp, ebp
pop ebp
ret 8

calculateGCD ENDP
main PROC
push ebp
mov ebp, esp
sub esp, 8
	push 20
	push 5
invoke calculateGCD
 push eax

	pop eax
	mov dword ptr[ebp-8], eax

mov eax, dword ptr[ebp-8] ; gcd
push eax
	push 20
	push 5
invoke calculateLCM
 push eax

	pop eax
	mov dword ptr[ebp-4], eax

mov eax, dword ptr[ebp-8] ; gcd
fn MessageBoxA,0, str$(eax), "Didenko Vladyslav IO-91", MB_OK

mov eax, dword ptr[ebp-4] ; lcm
fn MessageBoxA,0, str$(eax), "Didenko Vladyslav IO-91", MB_OK

add esp, 8
mov esp, ebp
pop ebp
ret 0

main ENDP
END _start

