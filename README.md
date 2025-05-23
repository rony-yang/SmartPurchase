# 쇼핑몰 구매내역 자동화 추출 프로그램
C# 개인 프로젝트

**1. 개발 목표** : 쇼핑몰의 구매내역을 자동으로 추출해주는 프로그램

**2. 메인 사진**
![Image](https://github.com/user-attachments/assets/9129a1ae-e90c-4c49-8c9c-4e8ad22a2d73)

![Image](https://github.com/user-attachments/assets/d8526ab2-e65f-439e-bddd-688495977955)

**3. 작업기간** : 2025. 5월

**4. 사용 기술**

- 언어 : C#

- 웹 개발 : HTML, CSS
  
- 개발 플랫폼 : .NET Framework
  
- IDE : visual studio

**5. 주요기능**

- 자동 구매 내역 확인 : 사용자가 로그인만 실행하면 프로그램이 자동으로 구매 내역을 확인

- 보안을 고려한 설계 : 로그인 정보를 프로그램에서 요구하거나 저장하지 않고, 사용자가 직접 로그인하여 보안 유지

- 구매내역 순서 정리 : 구매일자가 최근인 항목부터 순서대로 표시하며, 각 내역에 일련번호를 자동으로 부여

- 내역 요약 및 저장 : 구매일자와 상품명을 한 줄에 요약하여 보기 쉽게 메모장 파일에 정리하여 바탕화면에 저장함으로써 찾기 쉽도록 함

- 작업 과정 표시 : 프로그램이 실행되는 동안 현재 작업 단계를 화면에 표시하여 사용자가 진행 상황을 알아보기 쉽도록 함

**6. Advanced Feature**

보안 유지를 위해 로그인 정보를 받지 않고, 사용자가 직접 로그인을 실행하도록 하고 프로그램은 이를 기다리게 하였다.

이를 위해 세션 스토리지에 저장된 토큰(oms_token)값을 2초마다 검사하도록 구성하였다.

```C#
IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

// 로그인 대기 시간 타임아웃(3분 = 180초) 제한
int maxCheckSecond = 180;
int waitedSecond = 0;
bool isLoggedIn = false;

// 3분 동안 2초마다 토큰 값 확인
while (waitedSecond < maxCheckSecond)
{
    string omsToken = (string)js.ExecuteScript("return sessionStorage.getItem('oms_token');");
    // 로그인 성공
    if (!string.IsNullOrEmpty(omsToken))
    {
        isLoggedIn = true;
        break;
    }

    await Task.Delay(2000); // 2초 간격으로 다시 체크
    waitedSecond += 2;
}
