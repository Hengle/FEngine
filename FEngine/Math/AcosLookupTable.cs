﻿using System;

public static class AcosLookupTable
{
    public static readonly int COUNT = 0x400;
    public static readonly int HALF_COUNT = (COUNT >> 1);
    public static readonly int[] table = new int[] { 
        0x7ab8, 0x7847, 0x7744, 0x767d, 0x75d5, 0x7541, 0x74bb, 0x7440, 0x73ce, 0x7362, 0x72fc, 0x729b, 0x723f, 0x71e6, 0x7190, 0x713d, 
        0x70ed, 0x70a0, 0x7054, 0x700b, 0x6fc4, 0x6f7e, 0x6f3a, 0x6ef7, 0x6eb6, 0x6e76, 0x6e37, 0x6dfa, 0x6dbd, 0x6d82, 0x6d48, 0x6d0e, 
        0x6cd6, 0x6c9e, 0x6c67, 0x6c31, 0x6bfc, 0x6bc7, 0x6b93, 0x6b60, 0x6b2d, 0x6afb, 0x6ac9, 0x6a98, 0x6a68, 0x6a38, 0x6a09, 0x69da, 
        0x69ab, 0x697d, 0x6950, 0x6923, 0x68f6, 0x68ca, 0x689e, 0x6872, 0x6847, 0x681c, 0x67f2, 0x67c8, 0x679e, 0x6775, 0x674c, 0x6723, 
        0x66fa, 0x66d2, 0x66aa, 0x6683, 0x665b, 0x6634, 0x660d, 0x65e7, 0x65c0, 0x659a, 0x6575, 0x654f, 0x652a, 0x6504, 0x64df, 0x64bb, 
        0x6496, 0x6472, 0x644e, 0x642a, 0x6406, 0x63e3, 0x63c0, 0x639c, 0x637a, 0x6357, 0x6334, 0x6312, 0x62f0, 0x62cd, 0x62ac, 0x628a, 
        0x6268, 0x6247, 0x6226, 0x6204, 0x61e3, 0x61c3, 0x61a2, 0x6181, 0x6161, 0x6141, 0x6121, 0x6101, 0x60e1, 0x60c1, 0x60a1, 0x6082, 
        0x6063, 0x6043, 0x6024, 0x6005, 0x5fe6, 0x5fc8, 0x5fa9, 0x5f8b, 0x5f6c, 0x5f4e, 0x5f30, 0x5f11, 0x5ef4, 0x5ed6, 0x5eb8, 0x5e9a, 
        0x5e7d, 0x5e5f, 0x5e42, 0x5e24, 0x5e07, 0x5dea, 0x5dcd, 0x5db0, 0x5d93, 0x5d77, 0x5d5a, 0x5d3d, 0x5d21, 0x5d05, 0x5ce8, 0x5ccc, 
        0x5cb0, 0x5c94, 0x5c78, 0x5c5c, 0x5c40, 0x5c25, 0x5c09, 0x5bed, 0x5bd2, 0x5bb6, 0x5b9b, 0x5b80, 0x5b65, 0x5b49, 0x5b2e, 0x5b13, 
        0x5af8, 0x5ade, 0x5ac3, 0x5aa8, 0x5a8d, 0x5a73, 0x5a58, 0x5a3e, 0x5a23, 0x5a09, 0x59ef, 0x59d5, 0x59ba, 0x59a0, 0x5986, 0x596c, 
        0x5952, 0x5938, 0x591f, 0x5905, 0x58eb, 0x58d2, 0x58b8, 0x589f, 0x5885, 0x586c, 0x5852, 0x5839, 0x5820, 0x5807, 0x57ed, 0x57d4, 
        0x57bb, 0x57a2, 0x5789, 0x5770, 0x5758, 0x573f, 0x5726, 0x570d, 0x56f5, 0x56dc, 0x56c4, 0x56ab, 0x5693, 0x567a, 0x5662, 0x5649, 
        0x5631, 0x5619, 0x5601, 0x55e8, 0x55d0, 0x55b8, 0x55a0, 0x5588, 0x5570, 0x5558, 0x5540, 0x5529, 0x5511, 0x54f9, 0x54e1, 0x54ca, 
        0x54b2, 0x549a, 0x5483, 0x546b, 0x5454, 0x543c, 0x5425, 0x540e, 0x53f6, 0x53df, 0x53c8, 0x53b0, 0x5399, 0x5382, 0x536b, 0x5354, 
        0x533d, 0x5326, 0x530f, 0x52f8, 0x52e1, 0x52ca, 0x52b3, 0x529c, 0x5285, 0x526f, 0x5258, 0x5241, 0x522a, 0x5214, 0x51fd, 0x51e7, 
        0x51d0, 0x51b9, 0x51a3, 0x518c, 0x5176, 0x5160, 0x5149, 0x5133, 0x511c, 0x5106, 0x50f0, 0x50da, 0x50c3, 0x50ad, 0x5097, 0x5081, 
        0x506b, 0x5055, 0x503f, 0x5028, 0x5012, 0x4ffc, 0x4fe7, 0x4fd1, 0x4fbb, 0x4fa5, 0x4f8f, 0x4f79, 0x4f63, 0x4f4d, 0x4f38, 0x4f22, 
        0x4f0c, 0x4ef6, 0x4ee1, 0x4ecb, 0x4eb5, 0x4ea0, 0x4e8a, 0x4e75, 0x4e5f, 0x4e4a, 0x4e34, 0x4e1f, 0x4e09, 0x4df4, 0x4dde, 0x4dc9, 
        0x4db3, 0x4d9e, 0x4d89, 0x4d73, 0x4d5e, 0x4d49, 0x4d34, 0x4d1e, 0x4d09, 0x4cf4, 0x4cdf, 0x4cca, 0x4cb4, 0x4c9f, 0x4c8a, 0x4c75, 
        0x4c60, 0x4c4b, 0x4c36, 0x4c21, 0x4c0c, 0x4bf7, 0x4be2, 0x4bcd, 0x4bb8, 0x4ba3, 0x4b8e, 0x4b79, 0x4b64, 0x4b50, 0x4b3b, 0x4b26, 
        0x4b11, 0x4afc, 0x4ae7, 0x4ad3, 0x4abe, 0x4aa9, 0x4a95, 0x4a80, 0x4a6b, 0x4a56, 0x4a42, 0x4a2d, 0x4a19, 0x4a04, 0x49ef, 0x49db, 
        0x49c6, 0x49b2, 0x499d, 0x4989, 0x4974, 0x4960, 0x494b, 0x4937, 0x4922, 0x490e, 0x48f9, 0x48e5, 0x48d0, 0x48bc, 0x48a8, 0x4893, 
        0x487f, 0x486b, 0x4856, 0x4842, 0x482e, 0x4819, 0x4805, 0x47f1, 0x47dc, 0x47c8, 0x47b4, 0x47a0, 0x478c, 0x4777, 0x4763, 0x474f, 
        0x473b, 0x4727, 0x4712, 0x46fe, 0x46ea, 0x46d6, 0x46c2, 0x46ae, 0x469a, 0x4686, 0x4672, 0x465d, 0x4649, 0x4635, 0x4621, 0x460d, 
        0x45f9, 0x45e5, 0x45d1, 0x45bd, 0x45a9, 0x4595, 0x4581, 0x456d, 0x4559, 0x4546, 0x4532, 0x451e, 0x450a, 0x44f6, 0x44e2, 0x44ce, 
        0x44ba, 0x44a6, 0x4492, 0x447f, 0x446b, 0x4457, 0x4443, 0x442f, 0x441b, 0x4407, 0x43f4, 0x43e0, 0x43cc, 0x43b8, 0x43a4, 0x4391, 
        0x437d, 0x4369, 0x4355, 0x4342, 0x432e, 0x431a, 0x4306, 0x42f3, 0x42df, 0x42cb, 0x42b7, 0x42a4, 0x4290, 0x427c, 0x4269, 0x4255, 
        0x4241, 0x422e, 0x421a, 0x4206, 0x41f3, 0x41df, 0x41cb, 0x41b8, 0x41a4, 0x4190, 0x417d, 0x4169, 0x4155, 0x4142, 0x412e, 0x411a, 
        0x4107, 0x40f3, 0x40e0, 0x40cc, 0x40b8, 0x40a5, 0x4091, 0x407e, 0x406a, 0x4056, 0x4043, 0x402f, 0x401c, 0x4008, 0x3ff5, 0x3fe1, 
        0x3fcd, 0x3fba, 0x3fa6, 0x3f93, 0x3f7f, 0x3f6c, 0x3f58, 0x3f44, 0x3f31, 0x3f1d, 0x3f0a, 0x3ef6, 0x3ee3, 0x3ecf, 0x3ebc, 0x3ea8, 
        0x3e95, 0x3e81, 0x3e6d, 0x3e5a, 0x3e46, 0x3e33, 0x3e1f, 0x3e0c, 0x3df8, 0x3de5, 0x3dd1, 0x3dbe, 0x3daa, 0x3d97, 0x3d83, 0x3d6f, 
        0x3d5c, 0x3d48, 0x3d35, 0x3d21, 0x3d0e, 0x3cfa, 0x3ce7, 0x3cd3, 0x3cc0, 0x3cac, 0x3c99, 0x3c85, 0x3c72, 0x3c5e, 0x3c4a, 0x3c37, 
        0x3c23, 0x3c10, 0x3bfc, 0x3be9, 0x3bd5, 0x3bc2, 0x3bae, 0x3b9b, 0x3b87, 0x3b73, 0x3b60, 0x3b4c, 0x3b39, 0x3b25, 0x3b12, 0x3afe, 
        0x3aeb, 0x3ad7, 0x3ac3, 0x3ab0, 0x3a9c, 0x3a89, 0x3a75, 0x3a62, 0x3a4e, 0x3a3a, 0x3a27, 0x3a13, 0x3a00, 0x39ec, 0x39d8, 0x39c5, 
        0x39b1, 0x399d, 0x398a, 0x3976, 0x3963, 0x394f, 0x393b, 0x3928, 0x3914, 0x3900, 0x38ed, 0x38d9, 0x38c5, 0x38b2, 0x389e, 0x388a, 
        0x3877, 0x3863, 0x384f, 0x383c, 0x3828, 0x3814, 0x3800, 0x37ed, 0x37d9, 0x37c5, 0x37b2, 0x379e, 0x378a, 0x3776, 0x3763, 0x374f, 
        0x373b, 0x3727, 0x3713, 0x3700, 0x36ec, 0x36d8, 0x36c4, 0x36b0, 0x369d, 0x3689, 0x3675, 0x3661, 0x364d, 0x3639, 0x3626, 0x3612, 
        0x35fe, 0x35ea, 0x35d6, 0x35c2, 0x35ae, 0x359a, 0x3586, 0x3572, 0x355e, 0x354b, 0x3537, 0x3523, 0x350f, 0x34fb, 0x34e7, 0x34d3, 
        0x34bf, 0x34ab, 0x3497, 0x3483, 0x346e, 0x345a, 0x3446, 0x3432, 0x341e, 0x340a, 0x33f6, 0x33e2, 0x33ce, 0x33ba, 0x33a5, 0x3391, 
        0x337d, 0x3369, 0x3355, 0x3341, 0x332c, 0x3318, 0x3304, 0x32f0, 0x32db, 0x32c7, 0x32b3, 0x329f, 0x328a, 0x3276, 0x3262, 0x324d, 
        0x3239, 0x3225, 0x3210, 0x31fc, 0x31e7, 0x31d3, 0x31bf, 0x31aa, 0x3196, 0x3181, 0x316d, 0x3158, 0x3144, 0x312f, 0x311b, 0x3106, 
        0x30f2, 0x30dd, 0x30c9, 0x30b4, 0x309f, 0x308b, 0x3076, 0x3061, 0x304d, 0x3038, 0x3023, 0x300f, 0x2ffa, 0x2fe5, 0x2fd0, 0x2fbc, 
        0x2fa7, 0x2f92, 0x2f7d, 0x2f68, 0x2f54, 0x2f3f, 0x2f2a, 0x2f15, 0x2f00, 0x2eeb, 0x2ed6, 0x2ec1, 0x2eac, 0x2e97, 0x2e82, 0x2e6d, 
        0x2e58, 0x2e43, 0x2e2e, 0x2e19, 0x2e04, 0x2dee, 0x2dd9, 0x2dc4, 0x2daf, 0x2d9a, 0x2d84, 0x2d6f, 0x2d5a, 0x2d45, 0x2d2f, 0x2d1a, 
        0x2d04, 0x2cef, 0x2cda, 0x2cc4, 0x2caf, 0x2c99, 0x2c84, 0x2c6e, 0x2c59, 0x2c43, 0x2c2e, 0x2c18, 0x2c02, 0x2bed, 0x2bd7, 0x2bc2, 
        0x2bac, 0x2b96, 0x2b80, 0x2b6b, 0x2b55, 0x2b3f, 0x2b29, 0x2b13, 0x2afd, 0x2ae7, 0x2ad1, 0x2abb, 0x2aa5, 0x2a8f, 0x2a79, 0x2a63, 
        0x2a4d, 0x2a37, 0x2a21, 0x2a0b, 0x29f5, 0x29de, 0x29c8, 0x29b2, 0x299b, 0x2985, 0x296f, 0x2958, 0x2942, 0x292c, 0x2915, 0x28ff, 
        0x28e8, 0x28d1, 0x28bb, 0x28a4, 0x288e, 0x2877, 0x2860, 0x2849, 0x2833, 0x281c, 0x2805, 0x27ee, 0x27d7, 0x27c0, 0x27a9, 0x2792, 
        0x277b, 0x2764, 0x274d, 0x2736, 0x271f, 0x2708, 0x26f0, 0x26d9, 0x26c2, 0x26aa, 0x2693, 0x267c, 0x2664, 0x264d, 0x2635, 0x261e, 
        0x2606, 0x25ee, 0x25d7, 0x25bf, 0x25a7, 0x258f, 0x2577, 0x2560, 0x2548, 0x2530, 0x2518, 0x2500, 0x24e8, 0x24cf, 0x24b7, 0x249f, 
        0x2487, 0x246f, 0x2456, 0x243e, 0x2425, 0x240d, 0x23f4, 0x23dc, 0x23c3, 0x23ab, 0x2392, 0x2379, 0x2360, 0x2347, 0x232f, 0x2316, 
        0x22fd, 0x22e4, 0x22cb, 0x22b1, 0x2298, 0x227f, 0x2266, 0x224c, 0x2233, 0x2219, 0x2200, 0x21e6, 0x21cd, 0x21b3, 0x2199, 0x217f, 
        0x2166, 0x214c, 0x2132, 0x2118, 0x20fe, 0x20e3, 0x20c9, 0x20af, 0x2095, 0x207a, 0x2060, 0x2045, 0x202b, 0x2010, 0x1ff5, 0x1fda, 
        0x1fc0, 0x1fa5, 0x1f8a, 0x1f6f, 0x1f53, 0x1f38, 0x1f1d, 0x1f02, 0x1ee6, 0x1ecb, 0x1eaf, 0x1e93, 0x1e78, 0x1e5c, 0x1e40, 0x1e24, 
        0x1e08, 0x1dec, 0x1dd0, 0x1db3, 0x1d97, 0x1d7a, 0x1d5e, 0x1d41, 0x1d25, 0x1d08, 0x1ceb, 0x1cce, 0x1cb1, 0x1c93, 0x1c76, 0x1c59, 
        0x1c3b, 0x1c1e, 0x1c00, 0x1be2, 0x1bc4, 0x1ba6, 0x1b88, 0x1b6a, 0x1b4c, 0x1b2d, 0x1b0f, 0x1af0, 0x1ad1, 0x1ab3, 0x1a94, 0x1a75, 
        0x1a55, 0x1a36, 0x1a16, 0x19f7, 0x19d7, 0x19b7, 0x1997, 0x1977, 0x1957, 0x1937, 0x1916, 0x18f5, 0x18d4, 0x18b4, 0x1892, 0x1871, 
        0x1850, 0x182e, 0x180c, 0x17ea, 0x17c8, 0x17a6, 0x1784, 0x1761, 0x173e, 0x171b, 0x16f8, 0x16d5, 0x16b2, 0x168e, 0x166a, 0x1646, 
        0x1622, 0x15fd, 0x15d8, 0x15b4, 0x158e, 0x1569, 0x1543, 0x151e, 0x14f8, 0x14d1, 0x14ab, 0x1484, 0x145d, 0x1435, 0x140e, 0x13e6, 
        0x13be, 0x1395, 0x136c, 0x1343, 0x131a, 0x12f0, 0x12c6, 0x129c, 0x1271, 0x1246, 0x121a, 0x11ee, 0x11c2, 0x1195, 0x1168, 0x113b, 
        0x110d, 0x10de, 0x10af, 0x1080, 0x1050, 0x1020, 0xfef, 0xfbd, 0xf8b, 0xf58, 0xf25, 0xef1, 0xebc, 0xe87, 0xe51, 0xe1a, 
        0xde2, 0xdaa, 0xd70, 0xd36, 0xcfa, 0xcbe, 0xc81, 0xc42, 0xc02, 0xbc1, 0xb7e, 0xb3a, 0xaf4, 0xaad, 0xa63, 0xa18, 
        0x9cb, 0x97b, 0x928, 0x8d2, 0x879, 0x81d, 0x7bc, 0x756, 0x6ea, 0x677, 0x5fc, 0x577, 0x4e3, 0x43b, 0x374, 0x271, 
        0
     };

    static AcosLookupTable()
    {
        MDebug.Assert(table.Length == (COUNT + 1));
    }
}

