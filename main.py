def calculateLCM(m, n, gcd):
    return m * n / gcd


def calculateGCD(m, n):
    while m != n:
        if m > n:
            m -= n
        else:
            n -= m
    return n


def main():
    try:
        m = int(input())
        if m <= 0:
            raise ValueError
    except ValueError:
        print(f"m must be positive integer")
        raise ValueError
    try:
        n = int(input())
        if n <= 0:
            raise ValueError
    except ValueError:
        print(f"n must be positive integer")
        raise ValueError
    gcd = calculateGCD(m, n)
    lcm = calculateLCM(m, n, gcd)
    print(gcd)
    print(lcm)


if __name__ == "__main__":
    main()
