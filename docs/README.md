# Wait Your Turn — geliştirme kaydı

Güncelleme: 2026-10-07. Doküman dili Türkçe; kod, tip adları ve commit mesajları İngilizce.

Amaç: mobil zombi savunma oyununu küçük, doğrulanmış adımlarla geliştirmek; yeni mekanikler eklenirken önceki mekaniklerin korunmasını sağlamak.

## Okuma sırası

1. [Oyun tasarımı](GAME_DESIGN.md): oyuncunun deneyimi, kurallar, henüz kesinleşmemiş detaylar.
2. [Mimari](ARCHITECTURE.md): sorumluluklar, bağımlılıklar ve kritik teknik kurallar.
3. [Kararlar](DECISIONS.md): kararlaştırılanlar, öneriler ve açık sorular.
4. [Yol haritası](ROADMAP.md): sıralı geliştirme işleri ve kabul koşulları.
5. [Prototip başlangıç kaydı](BASELINE.md): mevcut yerel prototipin kaynak kontrolüne alınan durumu ve doğrulama sınırları.
6. [Navigasyon laboratuvarı](NAVIGATION_LAB.md): ilk uygulama, kontroller, doğrulama ve açık işler.

## Mevcut durum

- Uygulama başladı: ayrı NavigationSandbox sahnesinde kontrollü NavMesh geçişi kuruldu. Açık tasarım kararları `DECISIONS.md` içinde.
- 2026-10-07 revizyonu: M1 erken cihaz ölçümü, M3 minimum mobil kontrol/tabanca ve oynanış değerlendirmesi, M4a iki vagon → M4b beş vagon. Yeniden kullanım/kapsam sınırları ve erken tasarım kararları `ROADMAP.md` içinde.
- İncelenen Unity: 6000.6.4f1; kurulu AI Navigation: 2.0.14. Paket güncellemesi bu planın parçası değil.
- Referans/build sahnesi: `Assets/Scenes/SampleScene 2.unity`; yeni test sahnesi: `Assets/Game/Scenes/NavigationSandbox.unity`.
- Prototip: 5 rastgele oyuncu doğuş noktası, 6 düşman doğuş noktası, 7 saniyede bir üretim, 40 saniyede sahne yenileme.
- Vagon zorluğu kapı zamanlamasıyla oluşturuluyor. Gerçek kapı dayanıklılığı, oyuncu ölümü ve etap ilerlemesi henüz bağlı değil.
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

Reklam/IAP'nin ekonomi rolü M7 öncesinde tasarlanır; servis entegrasyonu ve yayınlama çekirdek oynanış/cihaz performansı doğrulandıktan sonraki aşamalardır.
