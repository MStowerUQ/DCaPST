﻿using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

namespace Validation.C4
{
    public static class SorghumData
    {
        public static IEnumerable<TestCaseData> HE_T1_Output
        {
            get
            {
                yield return new TestCaseData(36, -28.2099990844727, 29.3999996185303, 13.8999996185303, 27, 1, 1.40734632382729, 7.85179220587769, 0.587597180501773, 3.09742526374903, 1.27268165500234, 1.27268165500234, 9.087449082229, 3.09742526374903);
                yield return new TestCaseData(38, -28.2099990844727, 28.7999992370605, 14.1000003814697, 17, 1, 1.44812732169701, 9.15725441793213, 0.721725006852001, 3.3721891405678, 1.13149931006028, 1.13149931006028, 6.65065095986849, 3.3721891405678);
                yield return new TestCaseData(40, -28.2099990844727, 32.4000015258789, 14, 27, 1, 1.45535935837682, 8.83042607581023, 0.8802638834893, 4.81334041935083, 2.28948055253338, 2.28948055253338, 12.1212186655689, 4.81334041935083);
                yield return new TestCaseData(42, -28.2099990844727, 37, 19.3999996185303, 27, 0.33, 1.47291926813728, 8.23636477600067, 1.07580595109032, 9.50078673060512, 3.26921623512019, 3.26921623512019, 13.8120168045606, 9.50078673060512);
                yield return new TestCaseData(44, -28.2099990844727, 33.4000015258789, 19.8999996185303, 21, 0.33, 1.48662637510396, 8.35546178778733, 1.30132619749169, 10.2072026324344, 2.64836183933758, 2.64836183933758, 12.055061690451, 10.2072026324344);
                yield return new TestCaseData(46, -28.2099990844727, 26, 21.2999992370605, 11, 0.33, 1.45461620652201, 11.0871713236423, 1.58751991891913, 8.52938576990153, 0.95008464657402, 0.95008464657402, 7.05086256546685, 8.52938576990153);
                yield return new TestCaseData(48, -28.2099990844727, 30, 20.6000003814697, 17, 0.33, 1.44852789069668, 11.4529445938808, 1.87047176783952, 11.710955899338, 2.28335584139631, 2.28335584139631, 11.8352330362784, 11.710955899338);
                yield return new TestCaseData(50, -28.2099990844727, 27.1000003814697, 17.3999996185303, 23, 0.33, 1.45753674419864, 11.3874842400327, 2.1752215183948, 13.2052528429546, 2.83311745523225, 2.83311745523225, 17.1510092325741, 13.2052528429546);
                yield return new TestCaseData(52, -28.2099990844727, 30, 12.6999998092651, 22, 0.33, 1.47633709625708, 10.7219015351778, 2.42816866226199, 13.7719925932749, 3.67286617771122, 3.67286617771122, 17.1698351879911, 13.7719925932749);
                yield return new TestCaseData(54, -28.2099990844727, 34.7999992370605, 18.3999996185303, 25, 0.33, 1.49143430505911, 9.97685104778632, 2.65148141184033, 16.6249800633973, 5.06433807531313, 5.06433807531313, 20.1774555820998, 16.6249800633973);
                yield return new TestCaseData(56, -28.2099990844727, 30.2999992370605, 21.5, 16, 0.33, 1.49245491232149, 9.30080740125499, 2.89913394656425, 14.7682266375562, 2.78072680138692, 2.78072680138692, 13.3238613454882, 14.7682266375562);
                yield return new TestCaseData(58, -28.2099990844727, 24.2000007629395, 20.8999996185303, 12, 0.33, 1.48478428693445, 9.41465495451095, 3.15671209546371, 12.6144994289019, 1.32899653874103, 1.32899653874103, 10.2682869497477, 12.6144994289019);
                yield return new TestCaseData(60, -28.2099990844727, 30, 13.6999998092651, 25, 0.33, 1.48843889355271, 9.93004413119518, 3.36107619400388, 16.9398359580811, 4.70734011130756, 4.70734011130756, 21.7911159364673, 16.9398359580811);
                yield return new TestCaseData(62, -28.2099990844727, 31.5, 16.6000003814697, 25, 0.33, 1.49694350478261, 9.13429326506394, 3.51549267142973, 17.9720960198791, 5.02612774724557, 5.02612774724557, 22.0636834655827, 17.9720960198791);
                yield return new TestCaseData(64, -28.2099990844727, 27.3999996185303, 17.3999996185303, 11, 0.33, 1.49501054387716, 8.31659225690871, 3.73279878699623, 12.7588197894789, 2.0517733555028, 2.0517733555028, 9.85752533744785, 12.7588197894789);
                yield return new TestCaseData(66, -28.2099990844727, 25.7000007629395, 18.3999996185303, 12, 0.33, 1.48991580160852, 8.07296390843962, 3.93148040110876, 13.3782813389832, 1.89650953022344, 1.89650953022344, 10.8851799588113, 13.3782813389832);
                yield return new TestCaseData(68, -28.2099990844727, 25.2000007629395, 14.6000003814697, 21, 0.33, 1.48554098856194, 7.56223277927191, 4.10483434491298, 15.9921757181937, 3.33958125905783, 3.33958125905783, 19.2280090006358, 15.9921757181937);
                yield return new TestCaseData(70, -28.2099990844727, 25.2000007629395, 14.3999996185303, 18, 0.33, 1.48907560705616, 7.05283973394988, 4.10483434491298, 15.0129075862118, 2.91615683803768, 2.91615683803768, 16.4820591034857, 15.0129075862118);
                yield return new TestCaseData(72, -28.2099990844727, 27.6000003814697, 12, 24, 0.33, 1.48925200608269, 8.61131760311532, 4.10483434491298, 16.7013857589385, 4.3798738299956, 4.3798738299956, 21.9761985836018, 16.7013857589385);
                yield return new TestCaseData(74, -28.2099990844727, 29.8999996185303, 13.3999996185303, 24, 0.33, 1.48925585634848, 7.86515425681481, 4.10483434491298, 17.3121868362876, 4.84256823611539, 4.84256823611539, 21.9751188611389, 17.3121868362876);
                yield return new TestCaseData(76, -28.2099990844727, 29.3999996185303, 13.6999998092651, 22, 0.33, 1.48925585634848, 7.06223017077211, 4.10483434491298, 16.6513667260805, 4.38756104917277, 4.38756104917277, 20.1416698723751, 16.6513667260805);
                yield return new TestCaseData(78, -28.2099990844727, 30.1000003814697, 12.8000001907349, 23, 0.33, 1.48925585634848, 6.28444393512042, 4.10483434491298, 16.7867869478183, 4.71522013619536, 4.71522013619536, 21.0567416975107, 16.7867869478183);
                yield return new TestCaseData(80, -28.2099990844727, 33.2000007629395, 13, 23, 0.33, 1.48925585634848, 8.58178287506103, 4.10483434491298, 17.1629626084881, 5.36263478147872, 5.36263478147872, 21.0592794497954, 17.1629626084881);
                yield return new TestCaseData(82, -28.2099990844727, 28.2999992370605, 18.6000003814697, 17, 0.33, 1.48925585634848, 8.65503768920899, 4.10483434491298, 15.4695774344078, 3.11306132846235, 3.11306132846235, 15.5850818214943, 15.4695774344078);
                yield return new TestCaseData(84, -28.2099990844727, 26.7000007629395, 10.1000003814697, 23, 0.087, 1.48925585634848, 8.1700524520874, 4.10483434491298, 18.812731460988, 4.02789238843754, 4.02789238843754, 21.1359653809613, 18.812731460988);
                yield return new TestCaseData(86, -28.2099990844727, 27.3999996185303, 13.1999998092651, 21, 0.087, 1.48925585634848, 7.51842720031738, 4.10483434491298, 18.9321163732709, 3.79891098438604, 3.79891098438604, 19.3450000960964, 18.9321163732709);
                yield return new TestCaseData(88, -28.2099990844727, 27.8999996185303, 11.5, 20, 0.087, 1.48925585634848, 6.83330909729004, 4.10483434491298, 18.2781157425391, 3.78219046218777, 3.78219046218777, 18.4716321319948, 18.2781157425391);
                yield return new TestCaseData(90, -28.2099990844727, 27.2000007629395, 13.3999996185303, 19, 0, 1.48925585634848, 6.17405216217041, 4.10483434491298, 19.6120196553739, 3.43744992117057, 3.43744992117057, 17.5950667021609, 19.6120196553739);
                yield return new TestCaseData(92, -28.2099990844727, 29.1000003814697, 11.1000003814697, 18, 0, 1.48925585634848, 6.0567342376709, 4.10483434491298, 19.1024421877384, 3.69539954432555, 3.69539954432555, 16.7143634456712, 19.1024421877384);
                yield return new TestCaseData(94, -28.2099990844727, 29.2000007629395, 11, 19, 0, 1.48925585634848, 5.486955909729, 4.10483434491298, 19.3720822037989, 3.86705561031862, 3.86705561031862, 17.6912061279204, 19.3720822037989);
                yield return new TestCaseData(96, -28.2099990844727, 28.3999996185303, 7.40000009536743, 21, 0, 1.48925585634848, 4.81552282333374, 4.10483434491298, 19.0062842342788, 4.05520713734831, 4.05520713734831, 19.6068191379643, 19.0062842342788);
                yield return new TestCaseData(98, -28.2099990844727, 22.6000003814697, 11.8999996185303, 21, 0, 1.48925585634848, 4.10463088989258, 4.10483434491298, 17.8671404053972, 2.84401644969524, 2.84401644969524, 19.6598910365527, 17.8671404053972);
                yield return new TestCaseData(100, -28.2099990844727, 25.6000003814697, 9.19999980926514, 20, 0, 1.48925585634848, 5.43584224700928, 4.10483434491298, 17.9779890595739, 3.33472790738737, 3.33472790738737, 18.7736706876264, 17.9779890595739);
                yield return new TestCaseData(102, -28.2099990844727, 24.3999996185303, 12.8999996185303, 19, 0, 1.48925585634848, 4.8702770614624, 4.10483434491298, 17.9651641985306, 2.89601396063348, 2.89601396063348, 17.8816618070588, 17.9651641985306);
                yield return new TestCaseData(104, -28.2099990844727, 24.7999992370605, 7.69999980926514, 19, 0, 1.48925585634848, 4.34685348510742, 4.10483434491298, 16.784872852304, 3.04309772407448, 3.04309772407448, 17.9273286593864, 16.784872852304);
                yield return new TestCaseData(106, -28.2099990844727, 25, 13, 18, 0, 1.48925585634848, 3.77647426605225, 4.10483434491298, 17.5801681880185, 2.85305498014843, 2.85305498014843, 17.025910958024, 17.5801681880185);
                yield return new TestCaseData(108, -28.2099990844727, 28.3999996185303, 6.5, 18, 0, 1.48925585634848, 6.16795505523682, 4.10483434491298, 17.0500370335935, 3.54456137759572, 3.54456137759572, 17.066737039363, 17.0500370335935);
                yield return new TestCaseData(110, -28.2099990844727, 24.6000003814697, 13, 14, 0, 1.48925585634848, 5.61841400146484, 4.10483434491298, 15.6577742900435, 2.21663086391334, 2.21663086391334, 13.304760140483, 15.6577742900435);
                yield return new TestCaseData(112, -28.2099990844727, 27.2000007629395, 8.5, 19, 0, 1.48925585634848, 5.31597682952881, 4.10483434491298, 17.2425546810407, 3.45038609831692, 3.45038609831692, 18.0963768189361, 17.2425546810407);
                yield return new TestCaseData(114, -28.2099990844727, 26.2000007629395, 6.30000019073486, 18, 0, 1.48925585634848, 4.74936920166016, 4.10483434491298, 16.0291812264998, 3.12482372109859, 3.12482372109859, 17.1800780510123, 16.0291812264998);
                yield return new TestCaseData(116, -28.2099990844727, 22.5, 13.8999996185303, 16, 0, 1.48925585634848, 4.25526168823242, 4.10483434491298, 15.5014970104747, 2.09379170378394, 2.09379170378394, 15.3017405498911, 15.5014970104747);
                yield return new TestCaseData(118, -28.2099990844727, 24.3999996185303, 10.6000003814697, 17, 0, 1.48925585634848, 3.82496341705322, 4.10483434491298, 15.7708799655779, 2.62142345689679, 2.62142345689679, 16.2888382709947, 15.7708799655779);
                yield return new TestCaseData(120, -28.2099990844727, 23.5, 9.69999980926514, 16, 0, 1.48925585634848, 3.34943384170532, 4.10483434491298, 14.7430920314225, 2.33638056593517, 2.33638056593517, 15.357925019536, 14.7430920314225);
                yield return new TestCaseData(122, -28.2099990844727, 23, 12.3000001907349, 17, 0, 1.48925585634848, 4.9541389465332, 4.10483434491298, 15.3757085940389, 2.33749939947526, 2.33749939947526, 16.3449356563678, 15.3757085940389);
                yield return new TestCaseData(124, -28.2099990844727, 24.2999992370605, 13.3999996185303, 12, 0, 1.48925585634848, 4.59666786193848, 4.10483434491298, 14.0215297604706, 1.85112875156099, 1.85112875156099, 11.555463437965, 14.0215297604706);
                yield return new TestCaseData(126, -28.2099990844727, 23.8999996185303, 12.6999998092651, 13, 0, 1.47912974627155, 4.50669769287109, 4.10483434491298, 14.0127051429113, 1.93228088182865, 1.93228088182865, 12.5363616125222, 14.0127051429113);
                yield return new TestCaseData(128, -28.2099990844727, 18, 15.5, 8, 0, 1.46836295164426, 5.6216902923584, 4.10483434491298, 10.2849439556414, 0.577905082315087, 0.577905082315087, 7.72486454929857, 10.2849439556414);
                yield return new TestCaseData(130, -28.2099990844727, 18.7999992370605, 4.69999980926514, 16, 0, 1.41182128400421, 5.64475616455078, 4.05493747196244, 10.8870929589968, 1.55479486252694, 1.55479486252694, 15.446825605413, 10.8870929589968);
                yield return new TestCaseData(132, -28.2099990844727, 21.5, 6.80000019073486, 15, 0, 1.35847936601812, 5.32253944396973, 3.95760763532469, 11.4611508064663, 1.76608156426145, 1.76608156426145, 14.4571750510135, 11.4611508064663);
                yield return new TestCaseData(134, -28.2099990844727, 21.5, 8, 13, 0, 1.29687416483055, 5.01470478057861, 3.84470879404165, 10.4735600633337, 1.48091765212772, 1.48091765212772, 12.5000570380627, 10.4735600633337);
                yield return new TestCaseData(136, -28.2099990844727, 19, 14.6999998092651, 5, 0, 1.23255043646448, 4.85966945648193, 3.7332171713905, 7.59738842937225, 0.394701915594972, 0.394701915594972, 4.79535784470041, 7.59738842937225);
                yield return new TestCaseData(138, -28.2099990844727, 19, 9, 10, 0, 1.16817832467682, 5.32724018096924, 3.64842470516226, 7.93904580720917, 0.82310631638949, 0.82310631638949, 9.57263329020482, 7.93904580720917);
                yield return new TestCaseData(140, -28.2099990844727, 20.7000007629395, 3.5, 15, 0, 1.12905431833656, 5.17382183074951, 3.5653305334874, 8.41608058185247, 1.39521151274376, 1.39521151274376, 14.3300600927377, 8.41608058185247);
                yield return new TestCaseData(142, -28.2099990844727, 19.6000003814697, 7, 9, 0, 1.0886770551613, 4.96126289367676, 3.47749855184145, 6.89354122628273, 0.76756878494068, 0.76756878494068, 8.57741840685806, 6.89354122628273);
                yield return new TestCaseData(144, -28.2099990844727, 18, 11, 9, 0, 1.05240352126128, 4.8509001159668, 3.39665374836745, 6.68292776436571, 0.56250081714123, 0.56250081714123, 8.55736238687537, 6.68292776436571);
                yield return new TestCaseData(146, -28.2099990844727, 21.3999996185303, 3.09999990463257, 16, 0, 1.01298019208477, 4.61426738739014, 3.30655975718316, 7.51980491512388, 1.39756218494546, 1.39756218494546, 15.1679267351061, 7.51980491512388);
            }
        }
    }
}
