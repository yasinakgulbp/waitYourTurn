# Prototip başlangıç kaydı

Tarih: 2026-10-07. Kullanıcının bilgisayarındaki mevcut prototip, yeni mimariye geçmeden önce kaynak kontrolüne kaydedildi.

## Kaydedilen durum

- Unity 6000.6.4f1 (Unity 6.6); önceki Git sürümü 2023.2.6f1 idi.
- Aktif ve build listesinde etkin sahne: `Assets/Scenes/SampleScene 2.unity`.
- Yerelde bekleyen script/prefab/material/sahne değişiklikleri, yeni karakter/vagon/istasyon assetleri, sesler ve ilgili `.meta` dosyaları dahil edildi.
- `Packages` ve `ProjectSettings` değişiklikleri dahil edildi; aynı Unity sürümü ve paket bilgisiyle yeniden açılabilecek kaynak düzeni korundu.
- Plan belgeleri bir önceki ayrı committe kaydedildi.

## Git dışında tutulan yerel çıktılar

`BuildGate/`, `BuildGates2/` ve `The Gates Are Opening Harmony.rar` eski derlenmiş Windows oyun çıktılarıdır. Arşiv içeriğinde de BuildGates2 dağıtımı bulunur. Bu dosyalar silinmedi; `.gitignore` ile kaynak reposundan ayrı tutuldu. İlk bekleyen 412 dosyanın 289'u bu dağıtım çıktıları, 123'ü proje kaynakları/assetleri/ayarlarıydı.

## Doğrulama ve sınırlar

2026-10-07 tarihli önceki incelemede bilgisayardaki bu çalışma ağacı ve açık Unity sahnesi kullanıldı. Play modunda düşman üretimi, kapı döngüsü, ateş girişi ve sayaç sonrası yeniden yükleme/rastgele vagon değişimi incelendi; ardından Play kapatıldı.

Bu kayıt, çalışan prototipin başlangıç noktasıdır. Android build, uzun oturum/performance testi veya bütün sahnelerin hatasız çalıştığına dair doğrulama değildir. Bilinen eksikler (çoğalan AudioListener, oyuncu ölümü/etap sistemi eksikliği ve zamanlı kapılar) giderilmiş kabul edilmez; geliştirme planında takip edilir.

Bu adımda oynanış kodu düzeltilmedi. Bir sonraki iş M0/M1: prototipi koruyarak ayrı test sahnesinde kontrollü NavMesh kapı geçişini kurmak.
