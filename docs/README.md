# Wait Your Turn — geliştirme kaydı

Güncelleme: 2026-10-08. Doküman dili Türkçe; kod, tip adları ve commit mesajları İngilizce.

Amaç: mobil zombi savunma oyununu küçük, doğrulanmış adımlarla geliştirmek; yeni mekanikler eklenirken önceki mekaniklerin korunmasını sağlamak.

**Görsel hedef ilk prototiptir; TrainSandbox nihai oyun değildir.** Yeni geliştirmeden önce [görsel hedef ve entegrasyon denetimi](VISION_AND_INTEGRATION.md) okunur. M6a artık gerçek model/oranlarla `TrainIntegration` sahnesinde uygulanmıştır; iki/beş vagon PC teknik kabulü geçti. Bağlama kuralları, test kanıtı ve açık değerlendirmeler [TRAIN_INTEGRATION.md](TRAIN_INTEGRATION.md) içindedir. Sıradaki uygulama M6 spawn/tür/bütçe sistemidir.

## Okuma sırası

1. [Oyun tasarımı](GAME_DESIGN.md): oyuncunun deneyimi, kurallar, henüz kesinleşmemiş detaylar.
2. [Mimari](ARCHITECTURE.md): sorumluluklar, bağımlılıklar ve kritik teknik kurallar.
3. [Kararlar](DECISIONS.md): kararlaştırılanlar, öneriler ve açık sorular.
4. [Yol haritası](ROADMAP.md): sıralı geliştirme işleri ve kabul koşulları.
5. [Prototip başlangıç kaydı](BASELINE.md): mevcut yerel prototipin kaynak kontrolüne alınan durumu ve doğrulama sınırları.
6. [Navigasyon laboratuvarı](NAVIGATION_LAB.md): ilk uygulama, kontroller, doğrulama ve açık işler.
7. [Can/hasar laboratuvarı](COMBAT_LAB.md): ortak kurallar ve M2 kanıtı.
8. [Tek vagon oynanışı](GAMEPLAY_LAB.md): M3 entegrasyonu, kontroller ve açık cihaz değerlendirmesi.
9. [Vagon/koşu laboratuvarı](RUN_LAB.md): M4a iki vagon, M4b beş vagon; evre, kalıcılık, atama ve doğrulama sınırları.
10. [Silah laboratuvarı](WEAPONS_LAB.md): M5 ortak hitscan/pellet, dört silah tanımı, sınırlı yedek ve doğrulama.
11. [Görsel hedef ve entegrasyon](VISION_AND_INTEGRATION.md): referans prototip, somut lab varsayımları, M6a kabulü ve bot sırası.
12. [Gerçek vagon entegrasyonu](TRAIN_INTEGRATION.md): güncel sahne/kurulum, geometri sahipliği, iki taraflı altı kapı ve PC kabulü.

## Mevcut durum

- Uygulama başladı: ayrı NavigationSandbox sahnesinde kontrollü NavMesh geçişi kuruldu. Açık tasarım kararları `DECISIONS.md` içinde.
- 2026-10-07 revizyonu: M1 erken cihaz ölçümü, M3 minimum mobil kontrol/tabanca ve oynanış değerlendirmesi, M4a iki vagon → M4b beş vagon. Yeniden kullanım/kapsam sınırları ve erken tasarım kararları `ROADMAP.md` içinde.
- İncelenen Unity: 6000.6.4f1; kurulu AI Navigation: 2.0.14. Paket güncellemesi bu planın parçası değil.
- Referans/build sahnesi: `Assets/Scenes/SampleScene 2.unity`; ayrı test sahneleri: NavigationSandbox, CombatSandbox ve GameplaySandbox (`Assets/Game/Scenes`).
- Prototip: 5 rastgele oyuncu doğuş noktası, 6 düşman doğuş noktası, 7 saniyede bir üretim, 40 saniyede sahne yenileme.
- Eski prototip vagon zorluğunu kapı zamanlamasıyla oluşturur. Yeni GameplaySandbox'ta hasarlı kapı, onarım, oyuncuya saldırı/ölüm ve otomatik tabanca bağlıdır; istasyon/etap ilerlemesi henüz yoktur.
- İlk incelemede sahne yenileme/vagon değişimi görüldü. Altı zombi prefabındaki fazladan AudioListener bileşenleri M0 düzeltmesinde kaldırıldı; AudioSource ve eski oynanış scriptleri korundu.
- Plan öncesindeki yerel prototip değişiklikleri ayrı başlangıç commitine alındı. Eski Windows buildleri/arşivi yerelde korundu ve Git dışında tutuldu; plan commitine dahil edilmedi.

## Her geliştirme adımının çalışma biçimi

1. Yol haritasından tek bir aşama/iş seç; bağımlılıkları ve açık kararları oku.
2. Hangi davranışın değişeceğini ve aşamanın kabul koşullarını belirle.
3. Önce küçük test sahnesinde kur, ardından hedef oyun sahnesine entegre et.
4. İlgili mantık testlerini ve Play kontrollerini yap; performansı gereken aşamada cihazda ölç.
5. `ROADMAP.md` durumunu ve aşağıdaki kaydı güncelle. Kanıt olmadan işi tamamlandı işaretleme.
6. Yalnızca bu işe ait dosyaları commit et. Kullanıcının mevcut çalışmalarını izinsiz dahil etme. Commitler küçük ve İngilizce olsun.

Örnek commit: `feat: add damageable doors and repair interaction`.

Yeni bir oturumda bu dizin, güncel kod, Git durumu ve Unity sahnesi birlikte okunur. Plan belgesi tek başına uygulamanın kanıtı değildir. Değişen tasarım kararının gerekçesi `DECISIONS.md` içine eklenir; eski karar sessizce değiştirilmez.

## İlerleme kaydı

| Tarih | İş | Durum | Doğrulama / sonraki adım |
| --- | --- | --- | --- |
| 2026-10-07 | Prototip incelemesi | Tamamlandı | Kod/prefab incelemesi ve bir Play döngüsü; kaynak kod değiştirilmedi. |
| 2026-10-07 | Oyun tasarımı, mimari ve aşamalı plan | Hazır; açık kararlar var | İlk iş M0: prototip kaydı ve küçük navigasyon test sahnesi hazırlığı. |
| 2026-10-07 | Mevcut yerel prototipi kaynak kontrolüne alma | Tamamlandı | Kaynak/asset/ayarlar ayrı commit; eski dağıtım çıktıları Git dışında. M0/M1 test sahnesi henüz kurulmadı. |
| 2026-10-07 | Yararlı dış inceleme önerilerini plana işleme | Tamamlandı | Belgeler güncellendi; oynanış kodu ve Unity sahneleri değişmedi. Yeni aşama kabul koşulları henüz doğrulanmadı. |
| 2026-10-07 | NavMesh geçişi ve paralel kapı hatları | M1 lab kabulü tamamlandı | Son A54 APK'sı 10/10 PASS; 100/100 varış 36.25 saniye, eşzamanlı giriş 2. Kaba yük örnekleri yaklaşık 60 FPS; CPU örneği kaydedildi. Üretim entegrasyonu M3'te. Ayrıntılar NAVIGATION_LAB.md içinde. |
| 2026-10-07 | M0 geliştirme düzeni ve Android lab buildi | M0 tamamlandı | Referans sahne/zombi üretimi görüldü; AudioListener düzeltmesi ayrı commit. Android Development APK ve kimlik geri yüklemesi başarılı. A54'e kurulum/çalıştırma doğrulandı. |
| 2026-10-07 | Vagon kenarında kapı ve kesintisiz kalabalık geçişi | Tek-agent kontrolü 10/10; kalabalık hedef yerleşimi açık | Köprü/ikili link kaldırıldı; ortak NavMesh + carving. 30 agentta 29 hedefe varış, bir agent vagon içinde avoidance nedeniyle takıldı. M3/M6 hedef yönetimi işi; ayrıntı NAVIGATION_LAB.md. Rutin testler kullanıcı talebiyle PC'de. |
| 2026-10-07 | M2 ortak can, hasar ve ölüm | Tamamlandı | 14 EditMode testi geçti; hitscan, kapı 8+2, oyuncu iyileşmesi ve ölüm sinyali Play'de doğrulandı. COMBAT_LAB.md. Sıradaki M3: kapı kırılma/onarımı, enemy brain, minimum girdi/tabanca ve pool. |
| 2026-10-07 | M3 tek vagon mekaniği entegrasyonu (cam revizyonundan önce) | PC teknik kontrolü geçti; kullanıcı/cihaz değerlendirmesi açık | O sürüm 10/10 PASS: kapı hasarı/giriş, oyuncuya saldırı, iptal edilen/tam onarım, eşik güvenliği, pool yaşamı ve önceki kapı görüş engeli. Sol joystick/otomatik ateş ve otomatik reload kullanıcı tercihiyle kuruldu; GAMEPLAY_LAB.md. Android bu turda denenmedi. |
| 2026-10-07 | M3 kullanıcı geri bildirimi: cam kapı ve vagon sınırı | PC odaklı birleşik kontrol PASS | D15: sağlam/onarılmış camdan dış zombiye gerçek atış; alt panel/duvar engeli. D16: oyuncu sağlam/kırık/onarılmış kapıdan dışarı çıkamaz, zombi girişi korunur; görev için alan kaldırılabilir. Gerçek zombi kapı hasarı → kırılma → giriş → yakınlık onarımı da geçti. UTC 18:53:55; GAMEPLAY_LAB.md. Önceki 10 döngü bu revizyonda yeniden çalıştırılmadı; Android denenmedi. |

Reklam/IAP'nin ekonomi rolü M7 öncesinde tasarlanır; servis entegrasyonu ve yayınlama çekirdek oynanış/cihaz performansı doğrulandıktan sonraki aşamalardır.

2026-10-08: M4a iki vagonlu koşu sahnesi kuruldu. 10/10 geçiş + 6 saf evre testi PASS; normal iki-vagon saldırısı ve Play/GameOver kapanışında 0 hata doğrulandı. Kapı/oyuncu/mermi/iç-zombi kalıcılığı kanıtlandı; kullanıcı PC değerlendirmesini kabul etti. Battle/botsuz mod ve en az üç bot beceri profili tasarım hedefine eklendi, bot uygulaması yapılmadı. Google Play bulut kaydı aday; entegrasyon M8/servis aşamasında. Ayrıntı [RUN_LAB.md](RUN_LAB.md).

2026-10-08 M4b: aynı akış beş ayrı vagonla `TrainSandbox` sahnesinde kuruldu; yerleşim/kapı başlangıç canları TrainLayout verisinden uygulanır. 10/10 geçişte beş vagonun tümü ziyaret edildi ve durumlar korundu; 60 eşzamanlı zombiyle kısa PC yük kontrolü PASS (son kontrol: ortalama 4.82 ms/p95 7.38 ms, Editor tanısı). Kullanıcı teknik aşamayı kabul etti; gerçek altı kapılı tren, botlar ve bu sürümün Android kontrolü henüz yok. Geliştirmede tutarlı tekrar Play için standart domain/sahne reload açıldı; istasyon geçişleri hâlâ reload gerektirmez.

2026-10-08 M5: ortak HitscanWeapon/Resolver ve saf WeaponState kuruldu; dört tanım TrainSandbox'a bağlandı. Tabanca sınırsız yedek, SMG/tüfek/pompalı sınırlı yedek (D19). 13 EditMode testi, dört silahın gerçek cam/duvar/alt panel kontrolü ve seçili SMG/şarjör/yedeğin korunduğu 10/10 geçiş PASS. Hovl flash kozmetik kopyadan tekrar kullanılır, hasar ortak resolver'dan gelir. Kullanıcı/Android hissi ve denge açık; mağaza/turret/dron henüz yok. Ayrıntılar WEAPONS_LAB.md.

2026-10-08 D20 onarım devamı (`18a9bab`): hasarlı sağlam kapıya da süreli onarım; varsayılan oyuncu hasarında devam, kapı hasarında sıfırlama, iki ayrı ayar. 32 can/onarım vakası ve TrainSandbox kapı/işgal kontrolü PASS. Ayrıntı [GAMEPLAY_LAB.md](GAMEPLAY_LAB.md).

2026-10-08 D02 yolculuk sunumu tamamlaması: yalnız travers hareketi eksikti; istasyon platformu sabit colliderın görünümünden ayrıldı, yol zemini/çevresi de hareket eder. İstasyon döngülenmez, duruşta hizalanır; yeni istasyon tam karanlıkta yerleştirilir. 11 evre/hareket vakası ve beş-vagonda 10 geçiş PASS; collider/nav/spawn sabitliği denetlendi. D21 ile trenin iki tarafından saldırı hedefi kayda alındı; gerçek iki taraflı/altı kapılı geometri M6'da açık. Ayrıntı ve model bağlama kuralları [RUN_LAB.md](RUN_LAB.md), sahiplik [ARCHITECTURE.md](ARCHITECTURE.md), kesin kararlar [DECISIONS.md](DECISIONS.md). Android ve nihai görsel his değerlendirmesi ayrıca yapılacak.

2026-10-08 D22 hedef denetimi: ilk prototipin kamera/yerleşimi ile yeni kod karşılaştırıldı. Tek kapı enemy bağı, portal yarı düzleminden üyelik, sabit doğuş/iç noktalar ve vagon merkezine bağlı kamera gerçek geometri için açık uyarlamalardır. M6a ayrı ve sıradaki kabul olarak öne alındı; bu entegrasyon geçmeden yeni mağaza/turret/bot özelliğine geçilmeyecek. GAME_DESIGN'daki eski kırık-kapıya-özel onarım açıklaması D20 ile düzeltildi. Bu incelemede runtime/sahne değişikliği ve yeni Play testi yoktur. Ayrıntı [VISION_AND_INTEGRATION.md](VISION_AND_INTEGRATION.md).

2026-10-08 M6a uygulaması: kaynak prototip/prefab ve eski lablar korunarak TrainIntegration kuruldu. Referans model/oran, oyuncu X kamerası, vagon başına altı kapı, iki kesintisiz platform ve ayrı görsel hareket kullanılır. WagonGeometry gerçek iç hacim/doğuş/güvenli adayları sahiplenir; havuz erişilebilir kendi-vagon girişini seçer. Dar gerçek kapılar için ayrı Train Zombie agent türü ve yeni ortak NavMesh bake kullanılır; eski Humanoid ayarı korunur. İki-vagon 06:05:32 UTC, beş-vagon 06:08:40 UTC PC kabulü PASS: 10 geçiş, durumlar, iki taraflı eşik temizliği ve 24/60 düşmanlık havuz sınırı. 34 can/onarım/geometri ve 11 yolculuk testi PASS. Eşikte yakın oyuncuya saldırarak duran zombi sorunu, önce geçişi tamamlama önceliğiyle giderildi. Kullanıcı kadraj/his, Android/uzun oturum ve botlar açık; sıradaki uygulama M6. Ayrıntı [TRAIN_INTEGRATION.md](TRAIN_INTEGRATION.md).
