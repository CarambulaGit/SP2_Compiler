def calculateLCM(m, n, gcd):
    return m * n / gcd


def calculateGCD(m, n):
    while m != n:
        if m > n:
            m = m - n
        else:
            n = n - m
    return n


def main():
    gcd = calculateGCD(5, 20)
    lcm = calculateLCM(5, 20, gcd)
    print(gcd)
    print(lcm)


main()
