int calculateLCM(int m, int n, int gcd){
    return m * n / gcd;
}


int calculateGCD(int m, int n){
    while (m != n) {
        if (m > n) {
            m -= n;
        } else {
            n -= m;
        }
    }
    return n;
}


int main() {
    int gcd = calculateGCD(5, 20);
    int lcm = calculateLCM(5, 20, gcd);
    return 0;
}
