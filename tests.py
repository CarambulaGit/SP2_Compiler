import unittest

import main


class MyTestCase(unittest.TestCase):
    def test_lcm1(self):
        self.assertEqual(main.calculateLCM(3, 4, 1), 12)

    def test_lcm2(self):
        self.assertEqual(main.calculateLCM(6, 4, 2), 12)

    def test_lcm3(self):
        self.assertEqual(main.calculateLCM(6, 9, 3), 18)

    def test_lcm4(self):
        self.assertEqual(main.calculateLCM(10, 9, 1), 90)

    def test_lcm5(self):
        self.assertEqual(main.calculateLCM(25, 10, 5), 50)

    def test_gcd1(self):
        self.assertEqual(main.calculateGCD(3, 4), 1)

    def test_gcd2(self):
        self.assertEqual(main.calculateGCD(6, 4), 2)

    def test_gcd3(self):
        self.assertEqual(main.calculateGCD(6, 9), 3)

    def test_gcd4(self):
        self.assertEqual(main.calculateGCD(10, 9), 1)

    def test_gcd5(self):
        self.assertEqual(main.calculateGCD(25, 10), 5)


if __name__ == '__main__':
    unittest.main()
